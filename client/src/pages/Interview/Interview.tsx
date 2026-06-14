import { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { getInterview, generateFeedback } from '../../services/api';

import type { InterviewDto } from '../../types/api';
import Spinner from '../../components/Spinner/Spinner';
import MessageContent from '../../components/MessageContent/MessageContent';
import './Interview.css';

const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000/api/v1';

export default function Interview() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const abortRef = useRef<AbortController | null>(null);

  const [interview, setInterview] = useState<InterviewDto | null>(null);
  const [input, setInput] = useState('');
  const [sending, setSending] = useState(false);
  const [streamingMessage, setStreamingMessage] = useState<string | null>(null);
  const [pendingUserMessage, setPendingUserMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [showConfirm, setShowConfirm] = useState(false);
  const [ending, setEnding] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    getInterview(id)
      .then(setInterview)
      .catch(() => setError(t('common.error')))
      .finally(() => setLoading(false));
  }, [id]); // eslint-disable-line react-hooks/exhaustive-deps

  // Cleanup abort controller on unmount
  useEffect(() => {
    return () => { abortRef.current?.abort(); };
  }, []);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [interview?.messages, streamingMessage]);

  const candidateCount = interview?.messages?.filter(m => m.role === 'Candidate').length ?? 0;
  const canEnd = candidateCount >= 3;

  async function handleSend() {
    if (!id || !input.trim() || sending) return;

    const text = input.trim();
    setInput('');
    setSending(true);
    setStreamingMessage('');
    setPendingUserMessage(text);
    setError(null);

    abortRef.current = new AbortController();

    try {
      const response = await fetch(
        `${API_BASE}/interview/${id}/stream?content=${encodeURIComponent(text)}`,
        { signal: abortRef.current.signal }
      );

      if (!response.ok || !response.body) throw new Error('stream_failed');

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n');
        buffer = lines.pop() ?? '';

        for (const line of lines) {
          if (!line.startsWith('data: ')) continue;
          const data = line.slice(6).trim();

          if (data === '[DONE]') {
            const fresh = await getInterview(id);
            setPendingUserMessage(null);
            setInterview(fresh);
            setStreamingMessage(null);
            return;
          }

          try {
            const token: string = JSON.parse(data);
            setStreamingMessage(prev => (prev ?? '') + token);
          } catch {
            // ignore malformed SSE lines
          }
        }
      }
    } catch (err: unknown) {
      const isAbort = err instanceof DOMException && err.name === 'AbortError';
      if (!isAbort) setError(t('common.error'));
    } finally {
      setStreamingMessage(null);
      setPendingUserMessage(null);
      setSending(false);
    }
  }

  async function handleEnd() {
    if (!id) return;
    setShowConfirm(false);
    setEnding(true);

    try {
      await generateFeedback(id);
      navigate(`/interview/${id}/feedback`);
    } catch {
      setError(t('common.error'));
      setEnding(false);
    }
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  }

  useEffect(() => {
    if (interview?.status === 'Completed') {
      navigate(`/interview/${id}/feedback`, { replace: true });
    }
  }, [interview?.status, id, navigate]);

  if (loading) return <Spinner text={t('common.loading')} />;
  if (error && !interview) return <div className="upload-error">{error}</div>;
  if (!interview) return null;

  if (interview.status === 'Completed') return null;

  return (
    <div className="interview-page">
      <div className="interview-messages">
        {(interview.messages ?? []).map((msg) => (
          <div
            key={msg.id}
            className={`chat-message chat-message-${msg.role.toLowerCase()}`}
          >
            <div className={`chat-avatar chat-avatar-${msg.role.toLowerCase()}`}>
              {msg.role === 'Interviewer' ? 'AI' : t('interview.you')}
            </div>
            <div>
              <div className={`chat-bubble chat-bubble-${msg.role.toLowerCase()}`}>
                <MessageContent content={msg.content} />
              </div>
              <div className="chat-time">
                {new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </div>
            </div>
          </div>
        ))}

        {pendingUserMessage !== null && (
          <div className="chat-message chat-message-candidate">
            <div className="chat-avatar chat-avatar-candidate">{t('interview.you')}</div>
            <div>
              <div className="chat-bubble chat-bubble-candidate">
                <MessageContent content={pendingUserMessage} />
              </div>
            </div>
          </div>
        )}

        {sending && streamingMessage !== null && (
          <div className="chat-message chat-message-interviewer">
            <div className="chat-avatar chat-avatar-interviewer">AI</div>
            <div>
              <div className="chat-bubble chat-bubble-interviewer">
                {streamingMessage.length > 0
                  ? <MessageContent content={streamingMessage} />
                  : (
                    <div className="chat-thinking-dots">
                      <span className="chat-thinking-dot" />
                      <span className="chat-thinking-dot" />
                      <span className="chat-thinking-dot" />
                    </div>
                  )
                }
              </div>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      <div className="interview-input-area">
        {canEnd && !ending && (
          <button
            className="interview-end-btn"
            onClick={() => setShowConfirm(true)}
          >
            {t('interview.endInterview')}
          </button>
        )}

        {ending ? (
          <Spinner text={t('common.loading')} />
        ) : (
          <div className="interview-input-row">
            <textarea
              className="interview-input"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder={t('interview.placeholder')}
              disabled={sending}
              rows={1}
            />
            <button
              className="interview-send-btn"
              onClick={handleSend}
              disabled={!input.trim() || sending}
            >
              {t('interview.send')}
            </button>
          </div>
        )}
      </div>

      {showConfirm && (
        <div className="confirm-overlay" onClick={() => setShowConfirm(false)}>
          <div className="confirm-modal" onClick={(e) => e.stopPropagation()}>
            <p>{t('interview.endConfirm')}</p>
            <div className="confirm-actions">
              <button className="btn btn-secondary" onClick={() => setShowConfirm(false)}>
                {t('interview.cancel')}
              </button>
              <button className="btn btn-primary" onClick={handleEnd}>
                {t('interview.yes')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
