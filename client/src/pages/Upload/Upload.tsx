import { useState, useRef, useEffect, type DragEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { uploadCv, parseCv } from '../../services/api';
import Spinner from '../../components/Spinner/Spinner';
import './Upload.css';

const MAX_SIZE = 10 * 1024 * 1024; // 10MB

type AnalysisStep = 'pending' | 'active' | 'done';

export default function Upload() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const inputRef = useRef<HTMLInputElement>(null);

  const [file, setFile] = useState<File | null>(null);
  const [dragOver, setDragOver] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [analyzing, setAnalyzing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [steps, setSteps] = useState<[AnalysisStep, AnalysisStep, AnalysisStep]>(['pending', 'pending', 'pending']);

  useEffect(() => {
    if (!analyzing) return;
    setSteps(['active', 'pending', 'pending']);
    const t1 = setTimeout(() => setSteps(['done', 'active', 'pending']), 1500);
    const t2 = setTimeout(() => setSteps(['done', 'done', 'active']), 3000);
    return () => { clearTimeout(t1); clearTimeout(t2); };
  }, [analyzing]);

  function validate(f: File): string | null {
    if (f.type !== 'application/pdf') return t('upload.validationPdf');
    if (f.size > MAX_SIZE) return t('upload.validationSize');
    return null;
  }

  function handleFile(f: File) {
    const err = validate(f);
    if (err) { setError(err); return; }
    setError(null);
    setFile(f);
  }

  function handleDrop(e: DragEvent) {
    e.preventDefault();
    setDragOver(false);
    const dropped = e.dataTransfer.files[0];
    if (dropped) handleFile(dropped);
  }

  function handleInputChange(e: React.ChangeEvent<HTMLInputElement>) {
    const selected = e.target.files?.[0];
    if (selected) handleFile(selected);
  }

  function handleRemoveFile() {
    setFile(null);
    setError(null);
    if (inputRef.current) inputRef.current.value = '';
  }

  async function handleUpload() {
    if (!file) return;
    try {
      setUploading(true);
      setError(null);
      const cvId = await uploadCv(file);
      setUploading(false);
      setAnalyzing(true);
      await parseCv(cvId);
      navigate(`/profile/${cvId}`);
    } catch (err: unknown) {
      setUploading(false);
      setAnalyzing(false);
      const detail = (err as { detail?: string })?.detail;
      setError(detail ?? t('common.error'));
    }
  }

  const stepLabels = [
    t('upload.step1'),
    t('upload.step2'),
    t('upload.step3'),
  ];

  if (analyzing) {
    return (
      <div className="upload-page">
        <div className="upload-analyzing">
          <Spinner text={t('upload.analyzing')} />
          <p className="upload-analyzing-desc">{t('upload.analyzingDesc')}</p>
          <div className="analysis-steps">
            {stepLabels.map((label, i) => (
              <div key={i} className={`analysis-step analysis-step-${steps[i]}`}>
                <span className="analysis-step-indicator">
                  {steps[i] === 'done' ? (
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                      <polyline points="20 6 9 17 4 12" />
                    </svg>
                  ) : steps[i] === 'active' ? (
                    <span className="analysis-step-spinner" />
                  ) : (
                    <span className="analysis-step-number">{i + 1}</span>
                  )}
                </span>
                <span className="analysis-step-label">{label}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="upload-page">
      <h1>{t('upload.title')}</h1>
      <p className="upload-desc">{t('upload.desc')}</p>

      <div
        className={`dropzone ${dragOver ? 'drag-over' : ''}`}
        onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
        onDragLeave={() => setDragOver(false)}
        onDrop={handleDrop}
        onClick={() => inputRef.current?.click()}
      >
        <svg className="dropzone-icon" width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
          <polyline points="17 8 12 3 7 8" />
          <line x1="12" y1="3" x2="12" y2="15" />
        </svg>
        <p className="dropzone-text">{t('upload.dropzone')}</p>
        <p className="dropzone-or">{t('upload.dropzoneOr')}</p>
        <div className="dropzone-hints">
          <span className="dropzone-hint-pill">{t('upload.hintPdf')}</span>
          <span className="dropzone-hint-pill">{t('upload.hintSize')}</span>
        </div>
      </div>

      <input ref={inputRef} type="file" accept=".pdf" className="upload-input" onChange={handleInputChange} />

      {error && <p className="upload-error">{error}</p>}

      {file && !uploading && (
        <div className="upload-file-card">
          <div className="upload-file-info">
            <svg className="upload-file-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
              <polyline points="14 2 14 8 20 8" />
            </svg>
            <div>
              <p className="upload-file-name">{file.name}</p>
              <p className="upload-file-size">{(file.size / 1024).toFixed(0)} KB</p>
            </div>
            <button className="upload-file-remove" onClick={(e) => { e.stopPropagation(); handleRemoveFile(); }} aria-label="Remove file">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
          </div>
          <button className="btn btn-primary upload-submit-btn" onClick={handleUpload}>
            {t('upload.submit')}
          </button>
        </div>
      )}

      {uploading && <Spinner text={t('upload.uploading')} />}

      <p className="upload-privacy">{t('upload.privacy')}</p>
    </div>
  );
}
