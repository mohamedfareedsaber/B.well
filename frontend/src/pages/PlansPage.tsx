import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Users, User } from 'lucide-react';
import { PublicHeader } from '../components/Headers';
import { BrandLogo } from '../components/BrandLogo';
import { PlanCard } from '../components/PlanCard';
import { useGetPricingQuery } from '../services/bwellApi';
import { useAppDispatch } from '../app/hooks';
import { selectPlan } from '../features/checkout/checkoutSlice';
import type { PlanType } from '../types/api';
import './PlansPage.css';

export function PlansPage() {
  const [planType, setPlanType] = useState<PlanType>('Individual');
  const { data = [], isLoading, isError, error } = useGetPricingQuery(planType);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  return (
    <div className="plans-page">
      <PublicHeader />
      <main className="container plans-page__main">
        <div className="plans-hero">
          <BrandLogo size="lg" showWordmark={false} />
          <h1>B.Well Loyalty Cards</h1>
          <p>
            Choose the perfect membership plan for your healthcare needs. Get exclusive benefits,
            discounts, and priority access to our world-class medical services.
          </p>

          <div className="plan-toggle" role="tablist">
            <button
              type="button"
              className={planType === 'Individual' ? 'is-active' : ''}
              onClick={() => setPlanType('Individual')}
            >
              <User size={16} /> Individual
            </button>
            <button
              type="button"
              className={planType === 'Family' ? 'is-active' : ''}
              onClick={() => setPlanType('Family')}
            >
              <Users size={16} /> Family
            </button>
          </div>
        </div>

        {isLoading && <p className="state">Loading plans…</p>}
        {isError && <p className="state error">{(error as { data?: { message?: string } })?.data?.message || 'Failed to load plans'}</p>}

        <div className="plans-grid">
          {data.map((card) => (
            <PlanCard
              key={card.id}
              card={card}
              onSelect={() => {
                dispatch(
                  selectPlan({
                    planId: card.id,
                    planName: card.name,
                    planType,
                    priceEgp: card.priceEgp,
                    benefits: card.benefits,
                    colorGradient: card.colorGradient,
                    icon: card.icon,
                  }),
                );
                navigate('/checkout');
              }}
            />
          ))}
        </div>

        <p className="plans-footnote">All plans include access to our mobile app and online portal.</p>
      </main>
    </div>
  );
}
