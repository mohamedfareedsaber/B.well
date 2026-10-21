import './BrandLogo.css';

type Props = {
  size?: 'sm' | 'md' | 'lg';
  inverted?: boolean;
  showWordmark?: boolean;
};

export function BrandLogo({ size = 'md', inverted = false, showWordmark = true }: Props) {
  return (
    <div className={`brand brand--${size} ${inverted ? 'brand--inverted' : ''}`}>
      <span className="brand__mark" aria-hidden>
        <span className="brand__b" />
        <span className="brand__dot" />
      </span>
      {showWordmark && (
        <span className="brand__text">
          <strong>B.Well</strong>
          <em>clinics</em>
        </span>
      )}
    </div>
  );
}
