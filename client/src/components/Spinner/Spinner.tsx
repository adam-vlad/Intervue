import './Spinner.css';

interface SpinnerProps {
  text?: string;
}

export default function Spinner({ text }: SpinnerProps) {
  return (
    <div className="spinner-overlay">
      <div className="spinner" />
      {text && <p className="spinner-text">{text}</p>}
    </div>
  );
}
