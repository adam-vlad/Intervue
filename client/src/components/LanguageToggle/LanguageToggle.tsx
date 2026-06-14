import { useTranslation } from 'react-i18next';
import './LanguageToggle.css';

export default function LanguageToggle() {
  const { i18n } = useTranslation();

  const toggleLanguage = () => {
    const next = i18n.language === 'en' ? 'ro' : 'en';
    i18n.changeLanguage(next);
    localStorage.setItem('intervue-language', next);
  };

  return (
    <button
      className="language-toggle"
      onClick={toggleLanguage}
      aria-label="Toggle language"
    >
      {i18n.language === 'en' ? 'RO' : 'EN'}
    </button>
  );
}
