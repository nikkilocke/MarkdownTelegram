using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Markdig;
using Markdig.Extensions.Emoji;
using Markdig.Extensions.SelfPipeline;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax;

namespace MarkdownTelegram {
    public class TelegramPipelineBuilder : MarkdownPipelineBuilder {

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownPipeline" /> class.
        /// </summary>
        public TelegramPipelineBuilder() {
            var parser = InlineParsers.FindExact<EmphasisInlineParser>();
            if (parser != null)
                parser.EmphasisDescriptors.Add(new EmphasisDescriptor('~', 2, 2, true));

            InlineParsers.Add(new EmojiParser(EmojiMapping.DefaultEmojisAndSmileysMapping));
        }

    }
}
