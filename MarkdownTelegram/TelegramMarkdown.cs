using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Markdig;
using Markdig.Extensions.SelfPipeline;
using Markdig.Parsers;
using Markdig.Renderers.Roundtrip;
using Markdig.Syntax;
using Telegram.Bot.Types;

namespace MarkdownTelegram {
    public static class TelegramMarkdown  {
        public static readonly string Version = ((AssemblyFileVersionAttribute)typeof(Markdown).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0]).Version;

        internal static readonly MarkdownPipeline DefaultPipeline = new TelegramPipelineBuilder().Build();

#nullable enable
        private static MarkdownPipeline GetPipeline(MarkdownPipeline? pipeline, string markdown) {
#nullable disable
            if (pipeline == null)
                return DefaultPipeline;
            var selfPipeline = pipeline.Extensions.Find<SelfPipelineExtension>();
            if (selfPipeline != null)
                return selfPipeline.CreatePipelineFromInput(markdown);
            return pipeline;
        }

        public static MarkdownDocument ParseTelegram(string text, IEnumerable<MessageEntity> entities) {
            return null;
        }

        /// <summary>
        /// Converts a Markdown string to Telegram format.
        /// </summary>
        /// <param name="markdown">A Markdown text.</param>
        /// <param name="entities">Telegram MessageEntity list giving formatting information to pass to Telegram</param>
        /// <param name="pipeline">The pipeline used for the conversion.</param>
        /// <param name="context">A parser context used for the parsing.</param>
        /// <returns>The result of the conversion</returns>
        /// <exception cref="ArgumentNullException">if markdown variable is null</exception>
#nullable enable
        public static string ToTelegram(string markdown, List<MessageEntity> entities, MarkdownPipeline? pipeline = null, MarkdownParserContext? context = null) {
#nullable disable
            var document = Parse(markdown);
            return ToTelegram(document, entities);
        }

        /// <summary>
        /// Converts a Markdown document to HTML.
        /// </summary>
        /// <param name="document">A Markdown document.</param>
        /// <param name="entities">Telegram MessageEntity list giving formatting information to pass to Telegram</param>
        /// <param name="pipeline">The pipeline used for the conversion.</param>
        /// <returns>The result of the conversion</returns>
        /// <exception cref="ArgumentNullException">if markdown document variable is null</exception>
#nullable enable
        public static string ToTelegram(this MarkdownDocument document, List<MessageEntity> entities) {
#nullable disable
            if (document is null) throw new ArgumentNullException(nameof(document));
            StringBuilder result = new StringBuilder();
            TelegramRenderer renderer = new TelegramRenderer(result, entities);
            renderer.Render(document);
            return result.ToString();
        }

#nullable enable
        public static string ToMarkdown(this MarkdownDocument document) {
#nullable disable
            if (document is null) throw new ArgumentNullException(nameof(document));
            StringBuilder result = new StringBuilder();
            RoundtripRenderer renderer = new RoundtripRenderer(new StringWriter(result));
            renderer.Render(document);
            return result.ToString();
        }

        /// <summary>
        /// Parses the specified markdown into an AST <see cref="MarkdownDocument"/>
        /// </summary>
        /// <param name="markdown">The markdown text.</param>
        /// <param name="trackTrivia">Whether to parse trivia such as whitespace, extra heading characters and unescaped string values.</param>
        /// <returns>An AST Markdown document</returns>
        /// <exception cref="ArgumentNullException">if markdown variable is null</exception>
        public static MarkdownDocument Parse(string markdown) {
            if (markdown is null) throw new ArgumentNullException("markdown");

            MarkdownPipeline? pipeline = DefaultPipeline;

            return Markdown.Parse(markdown, pipeline);
        }


    }
}
