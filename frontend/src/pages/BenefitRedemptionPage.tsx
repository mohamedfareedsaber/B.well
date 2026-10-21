import { useMemo, useState, type FormEvent } from 'react';
import { Link, Navigate } from 'react-router-dom';
import {
  ArrowLeft,
  Check,
  Clock3,
  Gift,
  History,
  LogOut,
  Search,
  Sparkles,
} from 'lucide-react';
import { BrandLogo } from '../components/BrandLogo';
import { Button } from '../components/Button';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { logout } from '../features/auth/authSlice';
import {
  useGetMyBenefitRedemptionsQuery,
  useGetMyPortalQuery,
  useRequestBenefitRedemptionMutation,
} from '../services/bwellApi';
import './BenefitRedemptionPage.css';

export function BenefitRedemptionPage() {
  const token = useAppSelector((state) => state.auth.token);
  const role = useAppSelector((state) => state.auth.role);
  const dispatch = useAppDispatch();
  const { data: portal, isLoading: isPortalLoading, isError: isPortalError } =
    useGetMyPortalQuery(undefined, { skip: !token });
  const {
    data: redemptions = [],
    isLoading: areRedemptionsLoading,
    isError: areRedemptionsError,
  } = useGetMyBenefitRedemptionsQuery(undefined, { skip: !token });
  const [requestRedemption, { isLoading: isSubmitting, isError: submitError, isSuccess }] =
    useRequestBenefitRedemptionMutation();
  const [selectedBenefit, setSelectedBenefit] = useState('');
  const [memberId, setMemberId] = useState('');
  const [notes, setNotes] = useState('');
  const [search, setSearch] = useState('');
  const [historyFilter, setHistoryFilter] = useState<'All' | 'Pending' | 'Resolved'>('All');

  const eligibleBenefits =
    portal?.plan?.status === 'Active' ? portal.plan.benefits : [];
  const filteredBenefits = useMemo(
    () =>
      eligibleBenefits.filter((benefit) =>
        benefit.toLocaleLowerCase().includes(search.trim().toLocaleLowerCase()),
      ),
    [eligibleBenefits, search],
  );
  const filteredRedemptions = redemptions.filter((redemption) => {
    if (historyFilter === 'Pending') return redemption.status === 'Pending';
    if (historyFilter === 'Resolved') return redemption.status !== 'Pending';
    return true;
  });

  if (!token) return <Navigate to="/login" replace />;
  if (role === 'Admin') return <Navigate to="/admin" replace />;

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedBenefit) return;

    try {
      await requestRedemption({
        benefitName: selectedBenefit,
        familyMemberId: memberId || null,
        notes,
      }).unwrap();
      setSelectedBenefit('');
      setMemberId('');
      setNotes('');
    } catch {
      // The mutation state displays the API error without losing the user's form input.
    }
  }

  return (
    <div className="redemption-page">
      <header className="portal-header">
        <div className="container-wide portal-header__inner">
          <Link to="/portal" aria-label="Back to patient portal">
            <BrandLogo />
          </Link>
          <div className="portal-header__actions">
            <span className="portal-user">{portal?.fullName ?? 'Patient'}</span>
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

      <main className="container-wide redemption-main">
        <Link className="redemption-back" to="/portal">
          <ArrowLeft size={16} /> Back to my membership
        </Link>

        <section className="redemption-hero">
          <div className="redemption-hero__copy">
            <span className="redemption-overline"><Sparkles size={15} /> Member benefits</span>
            <h1>Redeem a benefit</h1>
            <p>Choose an included benefit and tell us who it’s for. You can follow every request below.</p>
          </div>
          <div className="redemption-membership">
            <span>Active membership</span>
            <strong>{portal?.plan?.planName ?? 'No active plan'}</strong>
            <small>{portal?.plan?.planType ?? 'Choose a plan to unlock benefits'}</small>
          </div>
        </section>

        {(isPortalLoading || areRedemptionsLoading) && (
          <p className="redemption-state" role="status">Loading your membership benefits…</p>
        )}
        {(isPortalError || areRedemptionsError) && (
          <p className="redemption-state redemption-state--error" role="alert">
            We couldn’t load your benefits or request history. Please refresh and try again.
          </p>
        )}

        {!isPortalLoading && portal && (
          <div className="redemption-layout">
            <section className="redemption-panel redemption-catalog">
              <div className="redemption-section-heading">
                <div>
                  <span className="redemption-step">01</span>
                  <h2>Choose a benefit</h2>
                </div>
                <span className="redemption-count">{eligibleBenefits.length} available</span>
              </div>

              {eligibleBenefits.length > 0 ? (
                <>
                  <label className="redemption-search">
                    <Search size={17} aria-hidden="true" />
                    <span className="visually-hidden">Search benefits</span>
                    <input
                      type="search"
                      placeholder="Search your benefits"
                      value={search}
                      onChange={(event) => setSearch(event.target.value)}
                    />
                  </label>
                  <div className="benefit-options">
                    {filteredBenefits.map((benefit, index) => {
                      const isSelected = selectedBenefit === benefit;
                      return (
                        <button
                          key={`${benefit}-${index}`}
                          type="button"
                          className={`benefit-option${isSelected ? ' is-selected' : ''}`}
                          aria-pressed={isSelected}
                          onClick={() => setSelectedBenefit(isSelected ? '' : benefit)}
                        >
                          <span className="benefit-option__icon"><Gift size={19} /></span>
                          <span className="benefit-option__text">
                            <strong>{benefit}</strong>
                            <small>Included with {portal.plan?.planName}</small>
                          </span>
                          <span className="benefit-option__check" aria-hidden="true">
                            {isSelected && <Check size={15} />}
                          </span>
                        </button>
                      );
                    })}
                    {!filteredBenefits.length && (
                      <p className="redemption-empty">No benefits match your search.</p>
                    )}
                  </div>
                </>
              ) : (
                <div className="redemption-empty">
                  <Gift size={23} />
                  <strong>No active membership benefits</strong>
                  <p>Benefits become available when your membership is active.</p>
                  {!portal.plan && <Link to="/plans">Explore membership plans</Link>}
                </div>
              )}
            </section>

            <section className="redemption-panel redemption-request-panel">
              <div className="redemption-section-heading">
                <div>
                  <span className="redemption-step">02</span>
                  <h2>Request redemption</h2>
                </div>
              </div>
              <form className="redemption-form" onSubmit={handleSubmit}>
                <label>
                  Benefit
                  <select
                    value={selectedBenefit}
                    onChange={(event) => setSelectedBenefit(event.target.value)}
                    required
                    disabled={!eligibleBenefits.length}
                  >
                    <option value="">Select a benefit</option>
                    {eligibleBenefits.map((benefit, index) => (
                      <option key={`${benefit}-${index}`} value={benefit}>{benefit}</option>
                    ))}
                  </select>
                </label>
                <label>
                  Redeem for
                  <select value={memberId} onChange={(event) => setMemberId(event.target.value)}>
                    <option value="">Me (primary member)</option>
                    {portal.familyMembers.map((member) => (
                      <option key={member.id} value={member.id}>{member.fullName}</option>
                    ))}
                  </select>
                </label>
                <label>
                  Additional details <span className="redemption-optional">Optional</span>
                  <textarea
                    value={notes}
                    onChange={(event) => setNotes(event.target.value)}
                    maxLength={1000}
                    rows={4}
                    placeholder="Add a note to help us process your request"
                  />
                  <small className="redemption-character-count">{notes.length}/1000</small>
                </label>
                {submitError && (
                  <p className="redemption-state--error" role="alert">
                    Your request could not be submitted. Please try again.
                  </p>
                )}
                {isSuccess && (
                  <p className="redemption-success" role="status">
                    <Check size={16} /> Request submitted. Its status is available in your history.
                  </p>
                )}
                <Button
                  type="submit"
                  variant="dark"
                  icon={<Gift size={17} />}
                  disabled={!selectedBenefit || isSubmitting}
                >
                  {isSubmitting ? 'Submitting…' : 'Submit request'}
                </Button>
                <p className="redemption-disclaimer">
                  Your request will be reviewed by the B.Well team. Submission does not guarantee approval.
                </p>
              </form>
            </section>
          </div>
        )}

        <section className="redemption-panel redemption-history">
          <div className="redemption-section-heading redemption-history__heading">
            <div>
              <span className="redemption-step"><History size={15} /></span>
              <h2>Redemption history</h2>
            </div>
            <div className="redemption-filters" aria-label="Filter redemption history">
              {(['All', 'Pending', 'Resolved'] as const).map((filter) => (
                <button
                  key={filter}
                  type="button"
                  className={historyFilter === filter ? 'is-active' : ''}
                  aria-pressed={historyFilter === filter}
                  onClick={() => setHistoryFilter(filter)}
                >
                  {filter}
                </button>
              ))}
            </div>
          </div>
          {areRedemptionsError ? null : filteredRedemptions.length ? (
            <div className="redemption-history-list">
              {filteredRedemptions.map((redemption) => (
                <article className="redemption-history-row" key={redemption.id}>
                  <span className="redemption-history-icon"><Gift size={18} /></span>
                  <div className="redemption-history-info">
                    <strong>{redemption.benefitName}</strong>
                    <span>
                      {redemption.memberName ?? 'Primary member'} ·{' '}
                      {new Date(redemption.createdAtUtc).toLocaleDateString()}
                    </span>
                    {redemption.notes && <p>{redemption.notes}</p>}
                  </div>
                  <span className={`redemption-status redemption-status--${redemption.status.toLowerCase()}`}>
                    {redemption.status === 'Pending' && <Clock3 size={14} />}
                    {redemption.status}
                  </span>
                </article>
              ))}
            </div>
          ) : (
            <div className="redemption-history-empty">
              <History size={20} />
              <p>{redemptions.length ? 'No requests in this filter.' : 'Your redemption requests will appear here.'}</p>
            </div>
          )}
        </section>
      </main>
    </div>
  );
}
