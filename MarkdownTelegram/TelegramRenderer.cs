using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MarkdownTelegram {
    /// <summary>
    /// Default HTML renderer for a Markdown <see cref="MarkdownDocument"/> object.
    /// </summary>
    /// <seealso cref="TextRendererBase{HtmlRenderer}" />
    public class TelegramRenderer : TextRendererBase<TelegramRenderer> {
        StringBuilder builder;
        List<MessageEntity> entities;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlRenderer"/> class.
        /// </summary>
        /// <param name="builder">The writer.</param>
        public TelegramRenderer(StringBuilder builder, List<MessageEntity> entities) : base(new StringWriter(builder)) {
            this.builder = builder;
            this.entities = entities;
            ObjectRenderers.Clear();
            // Default block renderers
            ObjectRenderers.Add(new CodeBlockRenderer());
            ObjectRenderers.Add(new ListRenderer());
            ObjectRenderers.Add(new HeadingRenderer());
            ObjectRenderers.Add(new HtmlBlockRenderer());
            ObjectRenderers.Add(new ParagraphRenderer());
            ObjectRenderers.Add(new QuoteBlockRenderer());
            ObjectRenderers.Add(new ThematicBreakRenderer());
            ObjectRenderers.Add(new LinkReferenceDefinitionGroupRenderer());
            ObjectRenderers.Add(new LinkReferenceDefinitionRenderer());
            ObjectRenderers.Add(new EmptyBlockRenderer());

            // Default inline renderers
            ObjectRenderers.Add(new AutolinkInlineRenderer());
            ObjectRenderers.Add(new CodeInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
            ObjectRenderers.Add(new TelegramHtmlInlineRenderer());
            ObjectRenderers.Add(new TelegramHtmlEntityInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());
        }

        public MessageEntity AddEntity(MessageEntityType type) {
            return AddEntity(new MessageEntity() { Type = type });
        }

        public MessageEntity AddEntity(MessageEntity entity) {
            entities.Add(entity);
            entity.Offset = builder.Length;
            return entity;
		}

        public MessageEntity EndEntity(MessageEntity entity) {
            entity.Length = builder.Length - entity.Offset;
            return entity;
        }

        /// <summary>
        /// Writes the lines of a <see cref="LeafBlock"/>
        /// </summary>
        /// <param name="leafBlock">The leaf block.</param>
        /// <returns>This instance</returns>
        public TelegramRenderer WriteLeafRawLines(LeafBlock leafBlock, bool writeEndOfLines, bool indent = false) {
            if (leafBlock is null) throw new ArgumentNullException("leafBlock");
            if (leafBlock.Lines.Lines != null) {
                var lines = leafBlock.Lines;
                var slices = lines.Lines;
                for (int i = 0; i < lines.Count; i++) {
                    if (!writeEndOfLines && i > 0) {
                        WriteLine();
                    }

                    if (indent) {
                        Write("    ");
                    }

                    Write(ref slices[i].Slice);

                    if (writeEndOfLines) {
                        WriteLine();
                    }
                }
            }
            return this;
        }

        public void RenderLinesBefore(Block block) {
            if (block.LinesBefore is null) {
                return;
            }
            foreach (var line in block.LinesBefore) {
                Write(line);
                WriteLine(line.NewLine);
            }
        }

        public void RenderLinesAfter(Block block) {
            previousWasLine = true;
            if (block.LinesAfter is null) {
                if (!IsLastInContainer)
                    WriteLine();
                return;
            }
            foreach (var line in block.LinesAfter) {
                Write(line);
                WriteLine(line.NewLine);
            }
        }

        public bool CompactParagraph { get; set; }

    }

    public abstract class TelegramObjectRenderer<TObject> : MarkdownObjectRenderer<TelegramRenderer, TObject> where TObject : MarkdownObject {
    }

}
