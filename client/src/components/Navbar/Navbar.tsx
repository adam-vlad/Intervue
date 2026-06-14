import { useState } from 'react';
import { Link, NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ThemeToggle from '../ThemeToggle/ThemeToggle';
import LanguageToggle from '../LanguageToggle/LanguageToggle';
import './Navbar.css';

export default function Navbar() {
  const { t } = useTranslation();
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <nav className="navbar">
      <Link to="/" className="navbar-brand">Intervue</Link>

      <div className="navbar-center">
        <NavLink to="/" end className={({ isActive }) => `navbar-tab ${isActive ? 'active' : ''}`}>
          {t('nav.home')}
        </NavLink>
        <NavLink to="/upload" className={({ isActive }) => `navbar-tab ${isActive ? 'active' : ''}`}>
          {t('nav.upload')}
        </NavLink>
        <NavLink to="/dashboard" className={({ isActive }) => `navbar-tab ${isActive ? 'active' : ''}`}>
          {t('nav.dashboard')}
        </NavLink>
      </div>

      <div className="navbar-actions">
        <LanguageToggle />
        <ThemeToggle />
      </div>

      <button
        className="navbar-hamburger"
        onClick={() => setMobileOpen(!mobileOpen)}
        aria-label="Toggle menu"
      >
        <span />
        <span />
        <span />
      </button>

      {mobileOpen && (
        <div className="navbar-mobile-menu">
          <NavLink to="/" end className={({ isActive }) => `navbar-tab ${isActive ? 'active' : ''}`} onClick={() => setMobileOpen(false)}>
            {t('nav.home')}
          </NavLink>
          <NavLink to="/upload" className={({ isActive }) => `navbar-tab ${isActive ? 'active' : ''}`} onClick={() => setMobileOpen(false)}>
            {t('nav.upload')}
          </NavLink>
          <NavLink to="/dashboard" className={({ isActive }) => `navbar-tab ${isActive ? 'active' : ''}`} onClick={() => setMobileOpen(false)}>
            {t('nav.dashboard')}
          </NavLink>
          <div className="navbar-actions">
            <LanguageToggle />
            <ThemeToggle />
          </div>
        </div>
      )}
    </nav>
  );
}
