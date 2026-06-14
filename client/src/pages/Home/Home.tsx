import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './Home.css';

export default function Home() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <div className="home">
      <section className="home-hero">
        <h1>{t('home.title')}</h1>
        <p className="home-subtitle">{t('home.subtitle')}</p>
        <p className="home-desc">{t('home.description')}</p>
        <button className="btn btn-primary" onClick={() => navigate('/upload')}>
          {t('home.cta')}
        </button>
      </section>

      <section className="home-steps">
        <div className="home-step">
          <span className="home-step-number">1</span>
          <div>
            <h3>{t('home.step1Title')}</h3>
            <p>{t('home.step1Desc')}</p>
          </div>
        </div>
        <div className="home-step-divider" />
        <div className="home-step">
          <span className="home-step-number">2</span>
          <div>
            <h3>{t('home.step2Title')}</h3>
            <p>{t('home.step2Desc')}</p>
          </div>
        </div>
        <div className="home-step-divider" />
        <div className="home-step">
          <span className="home-step-number">3</span>
          <div>
            <h3>{t('home.step3Title')}</h3>
            <p>{t('home.step3Desc')}</p>
          </div>
        </div>
      </section>
    </div>
  );
}
