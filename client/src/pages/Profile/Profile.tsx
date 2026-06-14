import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { getCvProfile, getInterviewsByCv, startInterview } from '../../services/api';
import type { CvProfileDto, InterviewSummaryDto } from '../../types/api';
import Spinner from '../../components/Spinner/Spinner';
import './Profile.css';

export default function Profile() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [profile, setProfile] = useState<CvProfileDto | null>(null);
  const [interviews, setInterviews] = useState<InterviewSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [starting, setStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showLanguageModal, setShowLanguageModal] = useState(false);
  const [selectedLanguage, setSelectedLanguage] = useState<'en' | 'ro'>('en');

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    Promise.all([getCvProfile(id), getInterviewsByCv(id)])
      .then(([p, i]) => { setProfile(p); setInterviews(i); })
      .catch(() => setError(t('common.error')))
      .finally(() => setLoading(false));
  }, [id]); // eslint-disable-line react-hooks/exhaustive-deps

  async function handleStartInterview() {
    if (!id) return;
    setShowLanguageModal(false);
    try {
      setStarting(true);
      const interview = await startInterview(id, selectedLanguage);
      navigate(`/interview/${interview.id}`);
    } catch {
      setError(t('common.error'));
      setStarting(false);
    }
  }

  if (loading) return <Spinner text={t('common.loading')} />;
  if (error || !profile) return <div className="upload-error">{error ?? t('common.error')}</div>;

  const difficultyClass = profile.difficultyLevel.toLowerCase();

  return (
    <div className="profile-page">
      <div className="profile-header">
        <h1>{t('profile.title')}</h1>
        <div className="profile-level">
          <span className="profile-level-label">{t('profile.difficultyLevel')}:</span>
          <span className={`badge badge-${difficultyClass}`}>
            {t(`difficulty.${profile.difficultyLevel}`)}
          </span>
        </div>
      </div>

      {profile.education && (
        <div className="profile-section">
          <h2>{t('profile.education')}</h2>
          <div className="profile-education card">{profile.education}</div>
        </div>
      )}

      {profile.technologies.length > 0 && (
        <div className="profile-section">
          <h2>{t('profile.technologies')}</h2>
          <div className="profile-technologies">
            {profile.technologies.map((tech) => (
              <span key={tech.name} className="profile-tech-chip">
                <span className="profile-tech-name">{tech.name}</span>
                <span className="profile-tech-years">
                  {tech.yearsOfExperience} {t('profile.years')}
                </span>
              </span>
            ))}
          </div>
        </div>
      )}

      {profile.experiences.length > 0 && (
        <div className="profile-section">
          <h2>{t('profile.experience')}</h2>
          <div className="profile-experience-list">
            {profile.experiences.map((exp, i) => (
              <div key={i} className="profile-experience-card card">
                <div className="profile-experience-header">
                  <div>
                    <span className="profile-experience-role">{exp.role}</span>
                    {' @ '}
                    <span className="profile-experience-company">{exp.company}</span>
                  </div>
                  <span className="profile-experience-duration">
                    {exp.durationMonths} {t('profile.months')}
                  </span>
                </div>
                {exp.description && (
                  <p className="profile-experience-desc">{exp.description}</p>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {profile.projects.length > 0 && (
        <div className="profile-section">
          <h2>{t('profile.projects')}</h2>
          <div className="profile-projects-grid">
            {profile.projects.map((proj, i) => (
              <div key={i} className="profile-project-card card">
                <p className="profile-project-name">{proj.name}</p>
                {proj.description && (
                  <p className="profile-project-desc">{proj.description}</p>
                )}
                {proj.technologiesUsed.length > 0 && (
                  <div className="profile-project-techs">
                    {proj.technologiesUsed.map((t) => (
                      <span key={t} className="profile-project-tech-tag">{t}</span>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="profile-cta">
        <button
          className="btn btn-primary"
          onClick={() => setShowLanguageModal(true)}
          disabled={starting}
        >
          {starting ? t('common.loading') : t('profile.startInterview')}
        </button>
      </div>

      {showLanguageModal && (
        <div className="profile-lang-overlay" onClick={() => setShowLanguageModal(false)}>
          <div className="profile-lang-modal" onClick={(e) => e.stopPropagation()}>
            <h3>{t('profile.chooseLanguage')}</h3>
            <p>{t('profile.chooseLanguageDesc')}</p>
            <div className="profile-lang-options">
              <button
                className={`profile-lang-option ${selectedLanguage === 'en' ? 'active' : ''}`}
                onClick={() => setSelectedLanguage('en')}
              >
                <span className="profile-lang-flag">🇬🇧</span>
                <span>{t('profile.languageEn')}</span>
              </button>
              <button
                className={`profile-lang-option ${selectedLanguage === 'ro' ? 'active' : ''}`}
                onClick={() => setSelectedLanguage('ro')}
              >
                <span className="profile-lang-flag">🇷🇴</span>
                <span>{t('profile.languageRo')}</span>
              </button>
            </div>
            <div className="profile-lang-actions">
              <button className="btn btn-secondary" onClick={() => setShowLanguageModal(false)}>
                {t('interview.cancel')}
              </button>
              <button className="btn btn-primary" onClick={handleStartInterview}>
                {t('profile.startIn')}
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="profile-section">
        <h2>{t('profile.pastInterviews')}</h2>
        {interviews.length === 0 ? (
          <p className="profile-no-interviews">{t('profile.noPastInterviews')}</p>
        ) : (
          <div className="profile-interviews-list">
            {interviews.map((iv) => (
              <div
                key={iv.id}
                className="profile-interview-item card card-clickable"
                onClick={() => navigate(
                  iv.hasFeedback ? `/interview/${iv.id}/feedback` : `/interview/${iv.id}`
                )}
              >
                <div className="profile-interview-left">
                  <span className={`badge status-${{ NotStarted: 'not-started', InProgress: 'in-progress', Completed: 'completed' }[iv.status] ?? iv.status.toLowerCase()}`}>
                    {t(`status.${iv.status}`)}
                  </span>
                  <span className="profile-interview-date">
                    {new Date(iv.startedAt).toLocaleDateString()}
                  </span>
                </div>
                {iv.overallScore != null && (
                  <span className="profile-interview-score">{iv.overallScore}/100</span>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
