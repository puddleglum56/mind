// Adapted from GitHub tiago-peres/blog https://github.com/tiago-peres/blog/blob/master/csvreader/CSVReader.cs

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CSVReader
{
	static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
	static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
	static char[] TRIM_CHARS = { '\"' };

	public static List<List<int>> Read(string file)
	{
		var list = new List<List<int>>();
		TextAsset data = Resources.Load(file) as TextAsset;

		var lines = Regex.Split(data.text, LINE_SPLIT_RE);

		if (lines.Length <= 1) return list;

		for (var i = 0; i < lines.Length; i++)
		{

			var values = Regex.Split(lines[i], SPLIT_RE);
			if (values.Length == 0 || values[0] == "") continue;

			var entry = new List<int>();
			for (var j = 0; j < values.Length; j++)
			{
				string value = values[j];
				value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
				Debug.Log(value);
				int finalvalue = int.Parse(value);
				entry.Add(finalvalue);
			}
			list.Add(entry);
		}
		return list;
	}
}
