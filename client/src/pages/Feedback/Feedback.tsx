import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { getInterview } from '../../services/api';
import type { InterviewDto } from '../../types/api';
import Spinner from '../../components/Spinner/Spinner';
import MessageContent from '../../components/MessageContent/MessageContent';
import './Feedback.css';

const RADIUS = 70;
const CIRCUMFERENCE = 2 * Math.PI * RADIUS;

export default function Feedback() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [interview, setInterview] = useState<InterviewDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    getInterview(id)
      .then(setInterview)
      .catch(() => setError(t('common.error')))
      .finally(() => setLoading(false));
  }, [id]); // eslint-disable-line react-hooks/exhaustive-deps

  if (loading) return <Spinner text={t('common.loading')} />;
  if (error || !interview?.feedbackReport) {
    return <div className="upload-error">{error ?? t('common.error')}</div>;
  }

  const fb = interview.feedbackReport;
  const dashOffset = CIRCUMFERENCE - (fb.overallScore / 100) * CIRCUMFERENCE;

  return (
    <div className="feedback-page">
      <h1>{t('feedback.title')}</h1>

      <div className="feedback-overall">
        <span className="feedback-overall-label">{t('feedback.overallScore')}</span>
        <div className="feedback-score-circle">
          <svg width="160" height="160" viewBox="0 0 160 160">
            <circle className="feedback-score-track" cx="80" cy="80" r={RADIUS} />
            <circle
              className="feedback-score-fill"
              cx="80"
              cy="80"
              r={RADIUS}
              strokeDasharray={CIRCUMFERENCE}
              strokeDashoffset={dashOffset}
            />
          </svg>
          <span className="feedback-score-value">{fb.overallScore}</span>
        </div>
      </div>

      <div className="feedback-categories">
        <h2>{t('feedback.categoryScores')}</h2>
        <div className="feedback-category-list">
          {fb.categoryScores.map((cat) => (
            <div key={cat.category} className="feedback-category-item">
              <span className="feedback-category-name">{cat.category}</span>
              <div className="feedback-category-bar-bg">
                <div
                  className="feedback-category-bar-fill"
                  style={{ '--bar-width': `${cat.score}%` } as React.CSSProperties}
                />
              </div>
              <span className="feedback-category-score">{cat.score}/100</span>
            </div>
          ))}
        </div>
      </div>

      <div className="feedback-cards-grid">
        <div className="feedback-section">
          <h2>{t('feedback.strengths')}</h2>
          <div className="feedback-section-card strengths">{fb.strengths}</div>
        </div>
        <div className="feedback-section">
          <h2>{t('feedback.weaknesses')}</h2>
          <div className="feedback-section-card weaknesses">{fb.weaknesses}</div>
        </div>
        <div className="feedback-section">
          <h2>{t('feedback.suggestions')}</h2>
          <div className="feedback-section-card suggestions">{fb.suggestions}</div>
        </div>
      </div>

      {interview.messages && interview.messages.length > 0 && (
        <div className="feedback-history">
          <h2>{t('feedback.conversationHistory')}</h2>
          <div className="feedback-history-messages">
            {interview.messages.map((msg) => (
              <div
                key={msg.id}
                className={`feedback-history-message feedback-history-message-${msg.role.toLowerCase()}`}
              >
                <div className={`feedback-history-avatar feedback-history-avatar-${msg.role.toLowerCase()}`}>
                  {msg.role === 'Interviewer' ? 'AI' : t('interview.you')}
                </div>
                <div>
                  <div className={`feedback-history-bubble feedback-history-bubble-${msg.role.toLowerCase()}`}>
                    <MessageContent content={msg.content} />
                  </div>
                  <div className="feedback-history-time">
                    {new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="feedback-actions">
        <button className="btn btn-secondary" onClick={() => navigate('/dashboard')}>
          {t('feedback.backToDashboard')}
        </button>
        <button className="btn btn-secondary" onClick={() => window.print()}>
          {t('feedback.downloadReport')}
        </button>
        <button className="btn btn-primary" onClick={() => navigate('/upload')}>
          {t('feedback.newInterview')}
        </button>
      </div>
    </div>
  );
}
