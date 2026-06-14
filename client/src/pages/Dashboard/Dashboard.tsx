import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Line, Doughnut, Bar } from 'react-chartjs-2';
import { getAllCvProfiles, getAllInterviews } from '../../services/api';
import type { CvProfileSummaryDto, InterviewSummaryDto } from '../../types/api';
import Spinner from '../../components/Spinner/Spinner';
import './Dashboard.css';

const CHART_COLORS = {
  junior: '#34d399',
  mid: '#f0b232',
  senior: '#a78bfa',
  accent: '#34d399',
  grid: 'rgba(107, 114, 128, 0.15)',
  label: '#787774',
};

export default function Dashboard() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [cvProfiles, setCvProfiles] = useState<CvProfileSummaryDto[]>([]);
  const [interviews, setInterviews] = useState<InterviewSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([getAllCvProfiles(), getAllInterviews()])
      .then(([cvs, ivs]) => { setCvProfiles(cvs); setInterviews(ivs); })
      .catch(() => setError(t('common.error')))
      .finally(() => setLoading(false));
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  if (loading) return <Spinner text={t('common.loading')} />;
  if (error) return <div className="upload-error">{error}</div>;

  const completedInterviews = interviews.filter(iv => iv.hasFeedback && iv.overallScore != null);
  const totalInterviews = interviews.length;
  const avgScore = completedInterviews.length > 0
    ? Math.round(completedInterviews.reduce((sum, iv) => sum + (iv.overallScore ?? 0), 0) / completedInterviews.length)
    : 0;
  const bestScore = completedInterviews.length > 0
    ? Math.max(...completedInterviews.map(iv => iv.overallScore ?? 0))
    : 0;

  const isEmpty = cvProfiles.length === 0 && interviews.length === 0;

  if (isEmpty) {
    return (
      <div className="dashboard-page">
        <div className="dashboard-empty">
          <svg className="dashboard-empty-icon" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
            <polyline points="14 2 14 8 20 8" />
            <line x1="16" y1="13" x2="8" y2="13" />
            <line x1="16" y1="17" x2="8" y2="17" />
            <polyline points="10 9 9 9 8 9" />
          </svg>
          <h2>{t('dashboard.emptyTitle')}</h2>
          <p>{t('dashboard.emptyDesc')}</p>
          <button className="btn btn-primary" onClick={() => navigate('/upload')}>
            {t('dashboard.uploadNew')}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="dashboard-page">
      <div className="dashboard-header">
        <h1>{t('dashboard.title')}</h1>
        <button className="btn btn-primary" onClick={() => navigate('/upload')}>
          {t('dashboard.uploadNew')}
        </button>
      </div>

      <div className="dashboard-stats">
        <div className="dashboard-stat card">
          <span className="dashboard-stat-value">{totalInterviews}</span>
          <span className="dashboard-stat-label">{t('dashboard.statsTotal')}</span>
        </div>
        <div className="dashboard-stat card">
          <span className="dashboard-stat-value">{avgScore}</span>
          <span className="dashboard-stat-label">{t('dashboard.statsAvg')}</span>
        </div>
        <div className="dashboard-stat card">
          <span className="dashboard-stat-value">{bestScore}</span>
          <span className="dashboard-stat-label">{t('dashboard.statsBest')}</span>
        </div>
      </div>

      <DashboardCharts
        cvProfiles={cvProfiles}
        completedInterviews={completedInterviews}
        t={t}
      />

      {cvProfiles.length > 0 && (
        <div className="dashboard-section">
          <h2>{t('dashboard.cvProfiles')}</h2>
          <div className="dashboard-cv-grid">
            {cvProfiles.map((cv) => (
              <div
                key={cv.id}
                className="dashboard-cv-card card card-clickable"
                onClick={() => navigate(`/profile/${cv.id}`)}
              >
                <div className="dashboard-cv-card-header">
                  <span className={`badge badge-${cv.difficultyLevel.toLowerCase()}`}>
                    {t(`difficulty.${cv.difficultyLevel}`)}
                  </span>
                  <span className="dashboard-cv-date">
                    {new Date(cv.createdAt).toLocaleDateString()}
                  </span>
                </div>
                {cv.technologies.length > 0 && (
                  <div className="dashboard-cv-techs">
                    {cv.technologies.slice(0, 5).map((tech) => (
                      <span key={tech} className="dashboard-cv-tech">{tech}</span>
                    ))}
                    {cv.technologies.length > 5 && (
                      <span className="dashboard-cv-tech">+{cv.technologies.length - 5}</span>
                    )}
                  </div>
                )}
                {cv.education && (
                  <div className="dashboard-cv-footer">{cv.education}</div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {interviews.length > 0 && (
        <div className="dashboard-section">
          <h2>{t('dashboard.recentInterviews')}</h2>
          <div className="dashboard-interview-list">
            {interviews.map((iv) => (
              <div
                key={iv.id}
                className="dashboard-interview-item card card-clickable"
                onClick={() => navigate(
                  iv.hasFeedback ? `/interview/${iv.id}/feedback` : `/interview/${iv.id}`
                )}
              >
                <div className="dashboard-interview-left">
                  <span className={`badge status-${{ NotStarted: 'not-started', InProgress: 'in-progress', Completed: 'completed' }[iv.status] ?? iv.status.toLowerCase()}`}>
                    {t(`status.${iv.status}`)}
                  </span>
                  <span className="dashboard-interview-date">
                    {new Date(iv.startedAt).toLocaleDateString()}
                  </span>
                  <span className="dashboard-interview-messages">
                    {iv.messageCount} {t('dashboard.msgSuffix')}
                  </span>
                </div>
                {iv.overallScore != null ? (
                  <span className="dashboard-interview-score">{iv.overallScore}/100</span>
                ) : (
                  <span className="dashboard-no-score">{t('dashboard.noScore')}</span>
                )}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

/* ── Chart sub-component ───────────────────────────────────────────── */

interface ChartsProps {
  cvProfiles: CvProfileSummaryDto[];
  completedInterviews: InterviewSummaryDto[];
  t: (key: string) => string;
}

function DashboardCharts({ cvProfiles, completedInterviews, t }: ChartsProps) {
  const sortedCompleted = useMemo(
    () => [...completedInterviews].sort((a, b) => new Date(a.startedAt).getTime() - new Date(b.startedAt).getTime()),
    [completedInterviews]
  );

  const scoreProgressData = useMemo(() => ({
    labels: sortedCompleted.map(iv =>
      new Date(iv.startedAt).toLocaleDateString([], { month: 'short', day: 'numeric' })
    ),
    datasets: [{
      label: t('dashboard.score'),
      data: sortedCompleted.map(iv => iv.overallScore as number),
      borderColor: CHART_COLORS.accent,
      backgroundColor: 'rgba(52, 211, 153, 0.1)',
      pointBackgroundColor: CHART_COLORS.accent,
      pointRadius: 5,
      tension: 0.4,
      fill: true,
    }],
  }), [sortedCompleted, t]);

  const difficultyData = useMemo(() => {
    const counts = {
      Junior: cvProfiles.filter(cv => cv.difficultyLevel === 'Junior').length,
      Mid: cvProfiles.filter(cv => cv.difficultyLevel === 'Mid').length,
      Senior: cvProfiles.filter(cv => cv.difficultyLevel === 'Senior').length,
    };
    return {
      labels: [t('difficulty.Junior'), t('difficulty.Mid'), t('difficulty.Senior')],
      datasets: [{
        data: [counts.Junior, counts.Mid, counts.Senior],
        backgroundColor: [
          'rgba(52, 211, 153, 0.8)',
          'rgba(240, 178, 50, 0.8)',
          'rgba(167, 139, 250, 0.8)',
        ],
        borderColor: [CHART_COLORS.junior, CHART_COLORS.mid, CHART_COLORS.senior],
        borderWidth: 1,
      }],
    };
  }, [cvProfiles, t]);

  const topTechData = useMemo(() => {
    const counts: Record<string, number> = {};
    cvProfiles.forEach(cv => {
      cv.technologies.forEach(name => {
        const key = name.toLowerCase();
        counts[key] = (counts[key] || 0) + 1;
      });
    });
    const top = Object.entries(counts)
      .sort((a, b) => b[1] - a[1])
      .slice(0, 8);
    return {
      labels: top.map(([name]) => name.charAt(0).toUpperCase() + name.slice(1)),
      datasets: [{
        label: t('dashboard.interviews'),
        data: top.map(([, count]) => count),
        backgroundColor: 'rgba(52, 211, 153, 0.7)',
        borderColor: CHART_COLORS.accent,
        borderWidth: 1,
      }],
    };
  }, [cvProfiles, t]);

  const lineOptions = useMemo(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: { backgroundColor: 'rgba(0,0,0,0.75)' },
    },
    scales: {
      x: {
        grid: { color: CHART_COLORS.grid },
        ticks: { color: CHART_COLORS.label, font: { size: 11 } },
      },
      y: {
        min: 0,
        max: 100,
        grid: { color: CHART_COLORS.grid },
        ticks: { color: CHART_COLORS.label, font: { size: 11 } },
      },
    },
  }), []);

  const donutOptions = useMemo(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom' as const,
        labels: { color: CHART_COLORS.label, font: { size: 11 }, padding: 16 },
      },
      tooltip: { backgroundColor: 'rgba(0,0,0,0.75)' },
    },
  }), []);

  const barOptions = useMemo(() => ({
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y' as const,
    plugins: {
      legend: { display: false },
      tooltip: { backgroundColor: 'rgba(0,0,0,0.75)' },
    },
    scales: {
      x: {
        grid: { color: CHART_COLORS.grid },
        ticks: { color: CHART_COLORS.label, font: { size: 11 }, stepSize: 1 },
      },
      y: {
        grid: { color: 'transparent' },
        ticks: { color: CHART_COLORS.label, font: { size: 11 } },
      },
    },
  }), []);

  const hasScoreProgress = sortedCompleted.length >= 2;
  const hasDifficulty = cvProfiles.length > 0;
  const hasTopTech = topTechData.labels.length > 0;

  if (!hasScoreProgress && !hasDifficulty && !hasTopTech) return null;

  return (
    <div className="dashboard-section">
      <div className="dashboard-charts">
        {hasScoreProgress && (
          <div className="dashboard-chart-card card">
            <h3 className="dashboard-chart-title">{t('dashboard.scoreProgress')}</h3>
            <div className="dashboard-chart-container">
              <Line data={scoreProgressData} options={lineOptions} />
            </div>
          </div>
        )}
        {hasDifficulty && (
          <div className="dashboard-chart-card card">
            <h3 className="dashboard-chart-title">{t('dashboard.difficultyDistribution')}</h3>
            <div className="dashboard-chart-container dashboard-chart-container-donut">
              <Doughnut data={difficultyData} options={donutOptions} />
            </div>
          </div>
        )}
        {hasTopTech && (
          <div className="dashboard-chart-card card">
            <h3 className="dashboard-chart-title">{t('dashboard.topTechnologies')}</h3>
            <div className="dashboard-chart-container">
              <Bar data={topTechData} options={barOptions} />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
