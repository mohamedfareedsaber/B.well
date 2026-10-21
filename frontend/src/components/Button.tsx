import type { ButtonHTMLAttributes, ReactNode } from 'react';
import './Button.css';

type Props = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'dark' | 'yellow' | 'danger' | 'ghost' | 'outline';
  icon?: ReactNode;
};

export function Button({ variant = 'dark', icon, children, className = '', ...rest }: Props) {
  return (
    <button className={`bw-btn bw-btn--${variant} ${className}`} {...rest}>
      {icon}
      <span>{children}</span>
    </button>
  );
}
