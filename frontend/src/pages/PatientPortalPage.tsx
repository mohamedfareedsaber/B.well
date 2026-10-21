import { Link, Navigate } from 'react-router-dom';
import { Calendar, Gift, Heart, LogOut, Users, Wallet } from 'lucide-react';
import { BrandLogo } from '../components/BrandLogo';
import { Button } from '../components/Button';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { logout } from '../features/auth/authSlice';
import { useGetMyPortalQuery } from '../services/bwellApi';
import './PatientPortalPage.css';

export function PatientPortalPage() {
  const token = useAppSelector((s) => s.auth.token);
  const role = useAppSelector((s) => s.auth.role);
  const dispatch = useAppDispatch();
  const { data, isLoading, isError } = useGetMyPortalQuery(undefined, { skip: !token });

  if (!token) return <Navigate to="/login" replace />;
  if (role === 'Admin') return <Navigate to="/admin" replace />;

  return (
    <div className="portal-page">
      <header className="portal-header">
        <div className="container-wide portal-header__inner">
          <BrandLogo />
          <div className="portal-header__actions">
            <Link to="/portal/redeem" className="portal-redeem-link">
              <Gift size={16} /> Redeem benefits
            </Link>
            <span className="portal-user">{data?.fullName ?? 'Patient'}</span>
            <Button
              variant="ghost"
              icon={<LogOut size={16} />}
              onClick={() => dispatch(logout())}
            >
              Logout
            </Button>
          </div>
        </div>
      </header>

      <main className="container-wide portal-main">
        {isLoading && <p className="state">Loading your membership…</p>}
        {isError && <p className="state error">Could not load your portal. Try signing in again.</p>}
        {data && (
          <>
            <section className="portal-hero">
              <div>
                <p className="eyebrow">My Membership</p>
                <h1>Welcome, {data.fullName.split(' ')[0]}</h1>
                <p className="muted">
                  {data.patientCode} · {data.mobileNumber}
                  {data.email ? ` · ${data.email}` : ''}
                </p>
              </div>
              {data.plan && (
                <div className="portal-plan-badge">
                  <strong>{data.plan.planName}</strong>
                  <span>
                    {data.plan.planType} · {data.plan.billingCycle}
                  </span>
                  <em>{data.plan.status}</em>
                </div>
              )}
            </section>

            <div className="portal-kpi">
              <article>
                <Calendar size={18} />
                <div>
                  <p>Visits this month</p>
                  <h2>{data.usage.visitsThisMonth}</h2>
                </div>
              </article>
              <article>
                <Heart size={18} />
                <div>
                  <p>Total visits</p>
                  <h2>{data.usage.totalVisits}</h2>
                </div>
              </article>
              <article>
                <Users size={18} />
                <div>
                  <p>Family members</p>
                  <h2>{data.usage.familyMemberCount}</h2>
                </div>
              </article>
              <article>
                <Wallet size={18} />
                <div>
                  <p>Est. savings</p>
                  <h2>E£{data.usage.estimatedSavingsEgp.toLocaleString()}</h2>
                </div>
              </article>
            </div>

            <div className="portal-grid">
              <section className="portal-card">
                <h3>My Plan</h3>
                {data.plan ? (
                  <>
                    <div className="plan-meta">
                      <span>Amount</span>
                      <strong>E£{data.plan.amountEgp.toLocaleString()}/year</strong>
                      <span>Next renewal</span>
                      <strong>
                        {data.plan.nextRenewalDateUtc
                          ? new Date(data.plan.nextRenewalDateUtc).toLocaleDateString()
                          : '—'}
                      </strong>
                    </div>
                    <h4>Benefits</h4>
                    <ul>
                      {data.plan.benefits.map((b) => (
                        <li key={b}>{b}</li>
                      ))}
                    </ul>
                    {!!data.plan.serviceQuotas.length && (
                      <>
                        <h4>Service usage & discounts</h4>
                        <div className="quota-list">
                          {data.plan.serviceQuotas.map((q) => (
                            <div key={q.serviceName} className="quota-row">
                              <strong>{q.serviceName}</strong>
                              <span>{q.visitCount} visits</span>
                              <small>
                                Discounts: {q.firstVisitDiscountPercent}% /{' '}
                                {q.secondVisitDiscountPercent}% / {q.additionalVisitDiscountPercent}%
                              </small>
                            </div>
                          ))}
                        </div>
                      </>
                    )}
                  </>
                ) : (
                  <p className="muted">
                    No active plan yet.{' '}
                    <Link to="/plans">Browse plans</Link>
                  </p>
                )}
              </section>

              <section className="portal-card">
                <h3>Family</h3>
                {!data.familyMembers.length && (
                  <p className="muted">No family members on this plan.</p>
                )}
                {data.familyMembers.map((m) => (
                  <div key={m.id} className="family-block">
                    <div className="family-head">
                      <strong>{m.fullName}</strong>
                      <span>
                        {[m.relationship, m.age != null ? `Age ${m.age}` : null]
                          .filter(Boolean)
                          .join(' · ')}
                      </span>
                      <em>{m.visitCount} visits</em>
                    </div>
                    {m.visits.length ? (
                      <ul className="visit-mini">
                        {m.visits.map((v) => (
                          <li key={v.id}>
                            <span>{v.medicalServiceName || 'Visit'}</span>
                            <span>{new Date(v.visitDateUtc).toLocaleDateString()}</span>
                            {v.discountAppliedPercent != null && (
                              <span>{v.discountAppliedPercent}% off</span>
                            )}
                          </li>
                        ))}
                      </ul>
                    ) : (
                      <p className="muted small">No visits recorded yet</p>
                    )}
                  </div>
                ))}
              </section>
            </div>

            <section className="portal-card">
              <h3>Recent visits</h3>
              {!data.recentVisits.length && <p className="muted">No visits yet.</p>}
              <div className="visit-table">
                {data.recentVisits.map((v) => (
                  <div key={v.id} className="visit-row">
                    <strong>{v.medicalServiceName || 'Visit'}</strong>
                    <span>{v.memberName}</span>
                    <span>{new Date(v.visitDateUtc).toLocaleDateString()}</span>
                    <span>
                      #{v.visitSequenceForService}
                      {v.discountAppliedPercent != null ? ` · ${v.discountAppliedPercent}% off` : ''}
                    </span>
                  </div>
                ))}
              </div>
            </section>

            {data.deliveryAddress && (
              <section className="portal-card">
                <h3>Delivery address</h3>
                <p>
                  {data.deliveryAddress.addressDetails}
                  <br />
                  {data.deliveryAddress.governorate}, {data.deliveryAddress.country}{' '}
                  {data.deliveryAddress.postalCode}
                </p>
              </section>
            )}
          </>
        )}
      </main>
    </div>
  );
}
