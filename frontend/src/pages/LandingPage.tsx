import { Link } from 'react-router-dom';
import { Award, BarChart3, CreditCard, Heart, Shield, Users } from 'lucide-react';
import { BrandLogo } from '../components/BrandLogo';
import { Button } from '../components/Button';
import { PublicHeader } from '../components/Headers';
import './LandingPage.css';

const features = [
  {
    icon: CreditCard,
    title: '4 Membership Tiers',
    text: 'Platinum, Gold, Silver, and Bronze options for every budget.',
  },
  {
    icon: Award,
    title: 'Smart Discounts',
    text: 'First visit free, second 50% off, ongoing tier benefits.',
  },
  {
    icon: Users,
    title: 'Family Plans',
    text: 'Add and manage multiple dependents under one account.',
  },
  {
    icon: BarChart3,
    title: 'Analytics Dashboard',
    text: 'Real-time tracking of subscriptions and revenue.',
  },
  {
    icon: Shield,
    title: 'Patient Management',
    text: 'Comprehensive database with visit history tracking.',
  },
  {
    icon: Heart,
    title: 'Revenue Insights',
    text: 'Visual data and performance metrics for admins.',
  },
];

export function LandingPage() {
  return (
    <div className="landing">
      <PublicHeader />

      <section className="landing-hero">
        <div className="landing-hero__badge">
          <Heart size={14} /> Healthcare Innovation • 2025
        </div>
        <BrandLogo size="lg" inverted showWordmark />
        <h1>Loyalty Program</h1>
        <p>
          A revolutionary healthcare membership platform with tiered benefits, smart discount
          systems, and comprehensive patient management
        </p>
        <div className="landing-hero__cta">
          <Link to="/plans">
            <Button variant="yellow">View Loyalty Cards</Button>
          </Link>
          <Link to="/login">
            <Button variant="outline">Admin Sign In</Button>
          </Link>
        </div>
        <div className="landing-hero__line" />
      </section>

      <section className="landing-features">
        <div className="container">
          <p className="eyebrow">PLATFORM FEATURES</p>
          <h2>Complete Healthcare Solution</h2>
          <div className="features-grid">
            {features.map((f) => (
              <article key={f.title} className="feature-card">
                <div className="feature-card__icon">
                  <f.icon size={22} />
                </div>
                <h3>{f.title}</h3>
                <p>{f.text}</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section className="landing-thanks">
        <div className="container landing-thanks__inner">
          <div className="landing-thanks__heart">
            <Heart size={28} />
          </div>
          <h2>Thank You</h2>
          <p>
            Making quality healthcare accessible through thoughtful design and innovative loyalty
            solutions.
          </p>
          <div className="thanks-stats">
            <div>
              <strong>4</strong>
              <span>MEMBERSHIP TIERS</span>
            </div>
            <div>
              <strong>6</strong>
              <span>KEY FEATURES</span>
            </div>
            <div>
              <strong>∞</strong>
              <span>BENEFITS</span>
            </div>
            <div>
              <strong>100%</strong>
              <span>FIRST VISIT</span>
            </div>
          </div>
          <p className="landing-thanks__copy">
            B.Well Clinics Loyalty Program © 2025 • Healthcare innovation
          </p>
        </div>
      </section>
    </div>
  );
}
