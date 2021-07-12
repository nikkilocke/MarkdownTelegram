using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using MarkdownTelegram;
using Xunit;
using System.IO;
using Xunit.Abstractions;
using Markdig;
using Markdig.Syntax;

namespace Tests {
	public class UnitTest1 {
		private readonly ITestOutputHelper output;

		public UnitTest1(ITestOutputHelper output) {
			this.output = output;
		}

		[Fact]
		public void MarkdownToTelegram() {
			foreach (string f in Directory.GetFiles("TestData", "*.md")) {
				RenderTelegram(System.IO.File.ReadAllText(f));
			}
		}

		void RenderTelegram(string text) {
			MarkdownDocument doc = TelegramMarkdown.Parse(text);
			output.WriteLine("=====telegram=====");
			List<MessageEntity> entities = new List<MessageEntity>();
			string result = TelegramMarkdown.ToTelegram(doc, entities);
			output.WriteLine(result);
			output.WriteLine($"Type:Offset:Length:Text:Url");
			foreach (MessageEntity e in entities) {
				output.WriteLine($"{e.Type}:{e.Offset}:{e.Length}:{result.Substring(e.Offset, e.Length)}:{e.Url}");
			}
			output.WriteLine("=====html=====");
			result = Markdown.ToHtml(doc);
			output.WriteLine(result);
			output.WriteLine("=====normalised=====");
			result = Markdown.Normalize(text);
			output.WriteLine(result);
		}

	}
}
