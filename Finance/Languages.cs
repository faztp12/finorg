﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinOrg
{
	public static class Languages
	{
		public static bool COPY_KEY_VALUES = true;

		// Set column name in TRANSLATIONS Table
		public static string currentLanguage = "english";

		// <DefaultValue, Translation>
		public static Dictionary<string, string> Translations;

		public static List<string> TranslationLoadedForms;

		//
		public static void Init()
		{
			new Thread(() => {
				SqlConnection con = FinOrgForm.getSqlConnection();
				try
				{
					con.Open();
					SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TRANSLATIONS';", con);
					if ((int)cmd.ExecuteScalar() == 0)
					{
						// create translations table
						cmd.CommandText = TRANSLATIONS_TABLE_SQL;
						cmd.ExecuteNonQuery();
					}

					con.Close();
				} catch (Exception e)
				{
					con.Close();
					MessageBox.Show(e.Message, "FinOrg Languages Init");
				}
			}).Start();
		}

		/// <summary>
		/// Lazy Loads the text for each form
		/// </summary>
		/// <param name="ControlDefaultValues">A dictionary with ControlName-Text pairs</param>
		public static Thread LazyLoadTranslations(Dictionary<string, string> ControlDefaultValues)
		{
			Thread t = new Thread(() =>
			{
				// Get a list of items to fetch
				IEnumerable<string> items = ControlDefaultValues.Values;
				if (Translations != null)
					items = items.Except(Translations.Keys).ToArray();
				SqlConnection con = FinOrgForm.getSqlConnection();
				try
				{
					con.Open();
					SqlCommand cmd = new SqlCommand(string.Format("SELECT text, {0} FROM TRANSLATIONS WHERE text IN ({{keys}});", currentLanguage), con);
					cmd.AddArrayParameters(items, "keys");
					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						if (Translations == null)
							Translations = new Dictionary<string, string>();
						while(reader.Read())
						{
							Translations.Add(reader["text"].ToString(), reader[currentLanguage].ToString());
						}
					}
					con.Close();
				}
				catch (Exception e)
				{
					con.Close();
					System.Windows.MessageBox.Show(e.Message, "FinOrg Languages LazyLoadTranslations");
				}
				if (COPY_KEY_VALUES) // Copies the value from form to database currentLanguage field
					InsertFormTranslations(ControlDefaultValues);
			});
			// finish  this fast
			t.Priority = ThreadPriority.Highest;
			t.Start();
			return t;
		}


		// Returns true if Control is translatable
		public static bool IsTranslatableControl(Control ctrl)
		{
			return new Type[] {
				typeof(Label),
				typeof(Button),
				typeof(CheckBox),
				typeof(RadioButton),
				typeof(MenuItem)
			}.Contains(ctrl.GetType());
		}


		/// <summary>
		/// Copies values
		/// Call from a thread, not from UI
		/// </summary>
		/// <param name="ControlDefaultValues"></param>
		public static void InsertFormTranslations(Dictionary<string, string> ControlDefaultValues)
		{
			if (ControlDefaultValues.Count <= 0)
				return;
			SqlConnection con = FinOrgForm.getSqlConnection();
			List<string> addedValues = new List<string>();
			try
			{
				con.Open();
				SqlCommand cmd = new SqlCommand(string.Format("INSERT INTO TRANSLATIONS (text, {0}) VALUES ", currentLanguage), con);
				int i = 0; // dictionary doesnt have proper index to loop with forloop
				foreach (KeyValuePair<string, string> e in ControlDefaultValues)
				{
					// check for duplications
					if (Translations.ContainsKey(e.Value) || addedValues.Contains(e.Value))
						continue;
					if (i > 0)
						cmd.CommandText += ", ";
					cmd.CommandText += string.Format("(@text{0}, @value{0})", i);
					cmd.Parameters.Add(new SqlParameter("@text" + i, e.Value));
					cmd.Parameters.Add(new SqlParameter("@value" + i, e.Value));
					addedValues.Add(e.Value);
					i++;
				}
				if (cmd.Parameters.Count > 0)
					cmd.ExecuteNonQuery();
				con.Close();
			} catch (Exception e)
			{
				con.Close();
				MessageBox.Show(e.Message, "FinOrg: Languages InsertFormTranslations");
			}
		}

		public static string TRANSLATIONS_TABLE_SQL = @"USE [Finance]
														GO

														/****** Object:  Table [dbo].[TRANSLATIONS]    Script Date: 20-Dec-17 3:06:26 PM ******/
														SET ANSI_NULLS ON
														GO

														SET QUOTED_IDENTIFIER ON
														GO

														CREATE TABLE [dbo].[TRANSLATIONS](
															[text] [nvarchar](200) NOT NULL,
															[english] [nvarchar](200) NULL,
															[arabic] [nvarchar](200) NULL,
														 CONSTRAINT [IX_TRANSLATIONS] UNIQUE NONCLUSTERED
														(
															[text] ASC
														)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
														) ON [PRIMARY]

														GO";
	}
}
