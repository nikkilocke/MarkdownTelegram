using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Globalization;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MarkdownTelegram {
    public class CodeBlockRenderer : TelegramObjectRenderer<CodeBlock> {
        protected override void Write(TelegramRenderer renderer, CodeBlock obj) {
            renderer.RenderLinesBefore(obj);
            renderer.Write(obj.TriviaBefore);
            MessageEntity entity = renderer.AddEntity(MessageEntityType.Pre);
            if (obj.Lines.Lines != null) {
                var lines = obj.Lines;
                var slices = lines.Lines;
                for (int i = 0; i < lines.Count; i++) {
                    ref StringSlice slice = ref slices[i].Slice;
                    renderer.Write(ref slice);
                    renderer.WriteLine(slice.NewLine);
                }
            }
            renderer.EndEntity(entity);
            renderer.Write(obj.TriviaAfter);
            renderer.RenderLinesAfter(obj);
        }

    }
    public class ListRenderer : TelegramObjectRenderer<ListBlock> {
        protected override void Write(TelegramRenderer renderer, ListBlock listBlock) {
            renderer.EnsureLine();
            var compact = renderer.CompactParagraph;
            renderer.CompactParagraph = !listBlock.IsLoose;
            renderer.RenderLinesBefore(listBlock);
            if (listBlock.IsOrdered) {
                int index = 0;
                if (listBlock.OrderedStart != null) {
                    switch (listBlock.BulletType) {
                        case '1':
                            int.TryParse(listBlock.OrderedStart, out index);
                            break;
                    }
                }
                for (var i = 0; i < listBlock.Count; i++) {
                    var item = listBlock[i];
                    var listItem = (ListItemBlock)item;
                    renderer.EnsureLine();

                    renderer.Write(index.ToString(CultureInfo.InvariantCulture));
                    renderer.Write(listBlock.OrderedDelimiter);
                    renderer.Write(' ');
                    renderer.PushIndent(new string(' ', IntLog10Fast(index) + 3));
                    renderer.WriteChildren(listItem);
                    renderer.PopIndent();
                    renderer.RenderLinesAfter(listItem);
                    switch (listBlock.BulletType) {
                        case '1':
                            index++;
                            break;
                    }
                    if (i + 1 < listBlock.Count && listBlock.IsLoose) {
                        renderer.EnsureLine();
                        renderer.WriteLine();
                    }
                }
            } else {
                for (var i = 0; i < listBlock.Count; i++) {
                    var item = listBlock[i];
                    var listItem = (ListItemBlock)item;
                    renderer.EnsureLine();
                    renderer.Write(listBlock.BulletType);
                    renderer.Write(' ');
                    renderer.PushIndent("  ");
                    renderer.WriteChildren(listItem);
                    renderer.PopIndent();
                    renderer.RenderLinesAfter(listItem);
                    if (i + 1 < listBlock.Count && listBlock.IsLoose) {
                        renderer.EnsureLine();
                        renderer.WriteLine();
                    }
                }
            }
            renderer.CompactParagraph = compact;
            renderer.RenderLinesAfter(listBlock);
        }

        private static int IntLog10Fast(int input) =>
             (input < 10) ? 0 :
             (input < 100) ? 1 :
             (input < 1000) ? 2 :
             (input < 10000) ? 3 :
             (input < 100000) ? 4 :
             (input < 1000000) ? 5 :
             (input < 10000000) ? 6 :
             (input < 100000000) ? 7 :
             (input < 1000000000) ? 8 : 9;
    }

    public class HeadingRenderer : TelegramObjectRenderer<HeadingBlock> {
        private static readonly string[] HeadingTexts = {
            "#",
            "##",
            "###",
            "####",
            "#####",
            "######",
        };

        protected override void Write(TelegramRenderer renderer, HeadingBlock obj) {
            var headingText = obj.Level > 0 && obj.Level <= 6
                ? HeadingTexts[obj.Level - 1]
                : new string('#', obj.Level);

            renderer.RenderLinesBefore(obj);
            renderer.Write(headingText).Write(' ');
            renderer.WriteLeafInline(obj);
            renderer.RenderLinesAfter(obj);
        }
    }

    public class HtmlBlockRenderer : TelegramObjectRenderer<HtmlBlock> {
        protected override void Write(TelegramRenderer renderer, HtmlBlock obj) {
            renderer.RenderLinesBefore(obj);
            renderer.WriteLeafRawLines(obj, true, false);
            renderer.RenderLinesAfter(obj);
        }
    }
    public class ParagraphRenderer : TelegramObjectRenderer<ParagraphBlock> {
        protected override void Write(TelegramRenderer renderer, ParagraphBlock paragraph) {
            renderer.RenderLinesBefore(paragraph);
            renderer.Write(paragraph.TriviaBefore);
            renderer.WriteLeafInline(paragraph);
            //renderer.Write(paragraph.Newline); // paragraph typically has LineBreakInlines as closing inline nodes
            renderer.RenderLinesAfter(paragraph);
        }
    }
    public class QuoteBlockRenderer : TelegramObjectRenderer<QuoteBlock> {
        protected override void Write(TelegramRenderer renderer, QuoteBlock quoteBlock) {
            renderer.RenderLinesBefore(quoteBlock);
            renderer.Write(quoteBlock.TriviaBefore);

            var indents = new string[quoteBlock.QuoteLines.Count];
            for (int i = 0; i < quoteBlock.QuoteLines.Count; i++) {
                var quoteLine = quoteBlock.QuoteLines[i];
                var wsb = quoteLine.TriviaBefore.ToString();
                var quoteChar = quoteLine.QuoteChar ? ">" : "";
                var spaceAfterQuoteChar = quoteLine.HasSpaceAfterQuoteChar ? " " : "";
                var wsa = quoteLine.TriviaAfter.ToString();
                indents[i] = (wsb + quoteChar + spaceAfterQuoteChar + wsa);
            }
            bool noChildren = false;
            if (quoteBlock.Count == 0) {
                noChildren = true;
                // since this QuoteBlock instance has no children, indents will not be rendered. We
                // work around this by adding empty LineBreakInlines to a ParagraphBlock.
                // Wanted: a more elegant/better solution (although this is not *that* bad).
                foreach (var quoteLine in quoteBlock.QuoteLines) {
                    var emptyLeafBlock = new ParagraphBlock {
                        NewLine = quoteLine.NewLine
                    };
                    var newLine = new LineBreakInline {
                        NewLine = quoteLine.NewLine
                    };
                    var container = new ContainerInline();
                    container.AppendChild(newLine);
                    emptyLeafBlock.Inline = container;
                    quoteBlock.Add(emptyLeafBlock);
                }
            }

            renderer.PushIndent(indents);
            renderer.WriteChildren(quoteBlock);
            renderer.PopIndent();

            if (!noChildren) {
                renderer.RenderLinesAfter(quoteBlock);
            }
        }
    }
    public class ThematicBreakRenderer : TelegramObjectRenderer<ThematicBreakBlock> {
        protected override void Write(TelegramRenderer renderer, ThematicBreakBlock obj) {
            renderer.RenderLinesBefore(obj);

            renderer.Write(obj.Content);
            renderer.WriteLine(obj.NewLine);
            renderer.RenderLinesAfter(obj);
        }
    }
    public class LinkReferenceDefinitionGroupRenderer : TelegramObjectRenderer<LinkReferenceDefinitionGroup> {
        protected override void Write(TelegramRenderer renderer, LinkReferenceDefinitionGroup obj) {
            renderer.WriteChildren(obj);
            renderer.RenderLinesAfter(obj);
        }
    }
    public class LinkReferenceDefinitionRenderer : TelegramObjectRenderer<LinkReferenceDefinition> {
        protected override void Write(TelegramRenderer renderer, LinkReferenceDefinition linkDef) {
            renderer.RenderLinesBefore(linkDef);

            renderer.Write(linkDef.TriviaBefore);
            renderer.Write('[');
            renderer.Write(linkDef.LabelWithTrivia);
            renderer.Write("]:");

            renderer.Write(linkDef.TriviaBeforeUrl);
            if (linkDef.UrlHasPointyBrackets) {
                renderer.Write('<');
            }
            renderer.Write(linkDef.UnescapedUrl);
            if (linkDef.UrlHasPointyBrackets) {
                renderer.Write('>');
            }

            renderer.Write(linkDef.TriviaBeforeTitle);
            if (linkDef.Title != null) {
                var open = linkDef.TitleEnclosingCharacter;
                var close = linkDef.TitleEnclosingCharacter;
                if (linkDef.TitleEnclosingCharacter == '(') {
                    close = ')';
                }
                renderer.Write(open);
                renderer.Write(linkDef.UnescapedTitle);
                renderer.Write(close);
            }
            renderer.Write(linkDef.TriviaAfter);
            renderer.Write(linkDef.NewLine.AsString());

            renderer.RenderLinesAfter(linkDef);
        }
    }
    public class EmptyBlockRenderer : TelegramObjectRenderer<EmptyBlock> {
        protected override void Write(TelegramRenderer renderer, EmptyBlock noBlocksFoundBlock) {
            renderer.RenderLinesAfter(noBlocksFoundBlock);
        }
    }
    public class AutolinkInlineRenderer : TelegramObjectRenderer<AutolinkInline> {
        protected override void Write(TelegramRenderer renderer, AutolinkInline obj) {
            MessageEntity entity = renderer.AddEntity(MessageEntityType.Url);
            entity.Url = obj.Url;
            if (obj.IsEmail)
                entity.Url = "mailto:" + entity.Url;
            renderer.Write(obj.Url);
            renderer.EndEntity(entity);
        }
    }

    public class CodeInlineRenderer : TelegramObjectRenderer<CodeInline> {
        protected override void Write(TelegramRenderer renderer, CodeInline obj) {
            MessageEntity entity = renderer.AddEntity(MessageEntityType.Code);
            renderer.Write(obj.Content);
            renderer.EndEntity(entity);
        }
    }

    public class DelimiterInlineRenderer : TelegramObjectRenderer<DelimiterInline> {
        protected override void Write(TelegramRenderer renderer, DelimiterInline obj) {
            renderer.Write(obj.ToLiteral());
            renderer.WriteChildren(obj);
        }
    }
    public class EmphasisInlineRenderer : TelegramObjectRenderer<EmphasisInline> {
        protected override void Write(TelegramRenderer renderer, EmphasisInline obj) {
            MessageEntityType type = MessageEntityType.Unknown;
            switch (obj.DelimiterChar) {
                case '*':
                    type = MessageEntityType.Bold;
                    break;
                case '_':
                    type = MessageEntityType.Italic;
                    break;
                case '~':
                    type = MessageEntityType.Strikethrough;
                    break;
                default:
                    renderer.WriteChildren(obj);
                    return;
            }
            MessageEntity entity = renderer.AddEntity(type);
            renderer.WriteChildren(obj);
            renderer.EndEntity(entity);
        }
    }

    public class LineBreakInlineRenderer : TelegramObjectRenderer<LineBreakInline> {
        protected override void Write(TelegramRenderer renderer, LineBreakInline obj) {
            renderer.WriteLine(obj.NewLine);
        }
    }
    public class TelegramHtmlInlineRenderer : TelegramObjectRenderer<HtmlInline> {
        protected override void Write(TelegramRenderer renderer, HtmlInline obj) {
            if(!renderer.ProcessHtmlTag(obj.Tag))
                renderer.Write(obj.Tag);
        }
    }
    public class TelegramHtmlEntityInlineRenderer : TelegramObjectRenderer<HtmlEntityInline> {
        protected override void Write(TelegramRenderer renderer, HtmlEntityInline obj) {
            renderer.Write(obj.Original);
        }
    }
    public class LinkInlineRenderer : TelegramObjectRenderer<LinkInline> {

        protected override void Write(TelegramRenderer renderer, LinkInline obj) {
            MessageEntity entity = renderer.AddEntity(MessageEntityType.Url);
            entity.Url = obj.Url;
            renderer.WriteChildren(obj);
            renderer.EndEntity(entity);
        }
    }

    public class LiteralInlineRenderer : TelegramObjectRenderer<LiteralInline> {
        protected override void Write(TelegramRenderer renderer, LiteralInline obj) {
            renderer.Write(ref obj.Content);
        }
    }
}
