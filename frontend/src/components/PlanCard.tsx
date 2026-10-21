import { Check } from 'lucide-react';
import { Button } from './Button';
import { ProgramIconView, tierGradient } from './icons';
import type { PricingCard } from '../types/api';
import './PlanCard.css';

type Props = {
  card: PricingCard;
  onSelect: () => void;
};

export function PlanCard({ card, onSelect }: Props) {
  return (
    <article className="plan-card">
      <div className="plan-card__head" style={{ background: tierGradient(card.colorGradient) }}>
        <div>
          <h3>{card.name}</h3>
          <p className="plan-card__price">
            E£{card.priceEgp.toLocaleString()} <span>/year</span>
          </p>
        </div>
        <ProgramIconView name={card.icon} size={28} color="#fff" />
      </div>
      <div className="plan-card__body">
        <p className="plan-card__desc">{card.description}</p>
        <ul>
          {card.benefits.map((b) => (
            <li key={b}>
              <Check size={16} color="var(--bw-success)" />
              <span>{b}</span>
            </li>
          ))}
          {card.extraBenefitsCount > 0 && (
            <li className="more">+{card.extraBenefitsCount} more benefit{card.extraBenefitsCount > 1 ? 's' : ''}</li>
          )}
        </ul>
        <Button variant="dark" className="plan-card__cta" onClick={onSelect}>
          Select Plan
        </Button>
      </div>
    </article>
  );
}
