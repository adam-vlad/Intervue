import { useMemo } from 'react';
import hljs from 'highlight.js';
import './MessageContent.css';

interface Props {
  content: string;
}

type Part =
  | { type: 'text'; text: string }
  | { type: 'code'; lang: string; html: string };

function parseContent(content: string): Part[] {
  const result: Part[] = [];
  const regex = /```(\w*)\n?([\s\S]*?)```/g;
  let lastIndex = 0;
  let match: RegExpExecArray | null;

  while ((match = regex.exec(content)) !== null) {
    if (match.index > lastIndex) {
      result.push({ type: 'text', text: content.slice(lastIndex, match.index) });
    }

    const lang = match[1] || 'plaintext';
    const code = match[2];
    let html: string;

    try {
      html = hljs.highlight(code, { language: lang, ignoreIllegals: true }).value;
    } catch {
      html = hljs.highlightAuto(code).value;
    }

    result.push({ type: 'code', lang, html });
    lastIndex = match.index + match[0].length;
  }

  if (lastIndex < content.length) {
    result.push({ type: 'text', text: content.slice(lastIndex) });
  }

  return result;
}

export default function MessageContent({ content }: Props) {
  const parts = useMemo(() => parseContent(content), [content]);

  return (
    <span className="message-content">
      {parts.map((part, i) =>
        part.type === 'code' ? (
          <pre key={i} className="message-code-block">
            <code dangerouslySetInnerHTML={{ __html: part.html }} />
          </pre>
        ) : (
          <span key={i}>{part.text}</span>
        )
      )}
    </span>
  );
}
