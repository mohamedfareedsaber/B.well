import { useMemo, useState } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { ArrowLeft, ArrowRight, Check, Plus, Users } from 'lucide-react';
import { PublicHeader } from '../components/Headers';
import { CheckoutStepper } from '../components/Stepper';
import { Button } from '../components/Button';
import { ProgramIconView, tierGradient } from '../components/icons';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import {
  addFamilyMember,
  patchCheckout,
  removeFamilyMember,
  setStep,
  toggleDisease,
  updateFamilyMember,
} from '../features/checkout/checkoutSlice';
import {
  useCompletePaymentMutation,
  useGetChronicDiseasesQuery,
  useGetGovernoratesQuery,
  useGetPaymentConfigQuery,
  useStartCheckoutMutation,
} from '../services/bwellApi';
import { getErrorMessage } from '../utils/errors';
import './CheckoutPage.css';

export function CheckoutPage() {
  const checkout = useAppSelector((s) => s.checkout);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { data: diseases = [] } = useGetChronicDiseasesQuery();
  const { data: governorates = [] } = useGetGovernoratesQuery();
  const { data: paymentConfig } = useGetPaymentConfigQuery();
  const [startCheckout, { isLoading: starting }] = useStartCheckoutMutation();
  const [completePayment, { isLoading: paying }] = useCompletePaymentMutation();
  const [error, setError] = useState<string | null>(null);

  if (!checkout.planId) return <Navigate to="/plans" replace />;

  const onNextPersonal = async () => {
    setError(null);
    if (!checkout.fullName.trim() || !checkout.mobileNumber.trim()) {
      setError('Full name and mobile number are required.');
      return;
    }
    if (checkout.planType === 'Family' && checkout.familyMembers.some((m) => !m.fullName.trim())) {
      setError('Please fill family member names or remove empty rows.');
      return;
    }
    dispatch(setStep(2));
  };

  const onNextMedical = () => {
    setError(null);
    dispatch(setStep(3));
  };

  const onPay = async () => {
    setError(null);
    if (!checkout.governorate || !checkout.postalCode || !checkout.addressDetails) {
      setError('Please complete the delivery address.');
      return;
    }

    try {
      const started = await startCheckout({
        loyaltyProgramId: checkout.planId,
        planType: checkout.planType,
        billingCycle: checkout.billingCycle,
        fullName: checkout.fullName,
        mobileNumber: checkout.mobileNumber,
        age: checkout.age ? Number(checkout.age) : null,
        email: checkout.email || null,
        nationality: checkout.nationality || null,
        nationalIdOrPassport: checkout.nationalIdOrPassport || null,
        chronicDiseaseIds: checkout.chronicDiseaseIds,
        medicalNotes: checkout.medicalNotes || null,
        familyMembers:
          checkout.planType === 'Family'
            ? checkout.familyMembers.map((m) => ({
                fullName: m.fullName,
                age: m.age ?? null,
                relationship: m.relationship || null,
                nationalIdOrPassport: m.nationalIdOrPassport || null,
              }))
            : [],
      }).unwrap();

      const paid = await completePayment({
        subscriptionId: started.subscriptionId,
        paymentMethod: checkout.paymentMethod,
        deliveryAddress: {
          country: checkout.country,
          governorate: checkout.governorate,
          postalCode: checkout.postalCode,
          addressDetails: checkout.addressDetails,
        },
      }).unwrap();

      dispatch(
        patchCheckout({
          subscriptionId: paid.subscriptionId,
          checkoutUrl: paid.checkoutUrl ?? null,
          clientSecret: paid.clientSecret ?? null,
          publicKey: paid.publicKey ?? null,
        }),
      );

      if (paid.checkoutUrl && paid.status !== 'Active') {
        window.location.href = paid.checkoutUrl;
        return;
      }

      if (paid.status === 'Active') {
        navigate(`/checkout/success/${paid.subscriptionId}`);
        return;
      }

      setError(
        'Paymob checkout URL was not returned. Add your Card Integration ID in Paymob settings (Dashboard → Settings → Payment Integrations).',
      );
    } catch (e) {
      setError(getErrorMessage(e, 'Payment failed'));
    }
  };

  const planSummary = useMemo(
    () => (
      <aside className="checkout-plan">
        <Link className="text-link" to="/plans">
          <ArrowLeft size={16} /> Back to Plans
        </Link>
        <div className="checkout-plan__card">
          <div className="checkout-plan__head" style={{ background: tierGradient(checkout.colorGradient || 'Gold') }}>
            <div>
              <h3>{checkout.planName} Plan</h3>
              <p className="price">
                E£{checkout.priceEgp.toLocaleString()} <span>/year</span>
              </p>
              <span className="type-tag">{checkout.planType}</span>
            </div>
            <ProgramIconView name={checkout.icon || 'Star'} size={28} color="#fff" />
          </div>
          <div className="checkout-plan__body">
            <h4>{checkout.planType === 'Family' ? 'Family Benefits' : 'Benefits'}</h4>
            <ul>
              {checkout.benefits.map((b) => (
                <li key={b}>
                  <Check size={16} color="var(--bw-success)" />
                  <span>{b}</span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </aside>
    ),
    [checkout],
  );

  return (
    <div className="checkout-page">
      <PublicHeader />
      <div className="container checkout-layout">
        {planSummary}
        <section className="checkout-form-card">
          <div className="checkout-form-card__head">
            <h2>Complete Your Subscription</h2>
            <p>Step {checkout.step} of 3</p>
          </div>
          <CheckoutStepper step={checkout.step} />

          {error && <div className="form-error">{error}</div>}

          {checkout.step === 1 && (
            <div className="form-block">
              <h3>Personal Information</h3>
              <label>
                Full Name *
                <input
                  value={checkout.fullName}
                  onChange={(e) => dispatch(patchCheckout({ fullName: e.target.value }))}
                  placeholder="Full name"
                />
              </label>
              <div className="grid-2">
                <label>
                  Mobile Number *
                  <input
                    value={checkout.mobileNumber}
                    onChange={(e) => dispatch(patchCheckout({ mobileNumber: e.target.value }))}
                    placeholder="+20 1xx xxx xxxx"
                  />
                </label>
                <label>
                  Age
                  <input
                    value={checkout.age}
                    onChange={(e) => dispatch(patchCheckout({ age: e.target.value }))}
                    placeholder="Age"
                  />
                </label>
              </div>
              <div className="grid-2">
                <label>
                  Nationality
                  <select
                    value={checkout.nationality}
                    onChange={(e) => dispatch(patchCheckout({ nationality: e.target.value }))}
                  >
                    <option>Egyptian</option>
                    <option>Other</option>
                  </select>
                </label>
                <label>
                  National ID / Passport
                  <input
                    value={checkout.nationalIdOrPassport}
                    onChange={(e) => dispatch(patchCheckout({ nationalIdOrPassport: e.target.value }))}
                  />
                </label>
              </div>
              <label>
                Email
                <input
                  type="email"
                  value={checkout.email}
                  onChange={(e) => dispatch(patchCheckout({ email: e.target.value }))}
                  placeholder="name@example.com"
                />
              </label>

              {checkout.planType === 'Family' && (
                <div className="family-block">
                  <div className="family-block__head">
                    <h4>
                      <Users size={16} /> Family Members
                    </h4>
                    <Button type="button" variant="outline" icon={<Plus size={16} />} onClick={() => dispatch(addFamilyMember())}>
                      Add Member
                    </Button>
                  </div>
                  {checkout.familyMembers.length === 0 ? (
                    <p className="empty-hint">Click &apos;Add Member&apos; to include family members in your plan.</p>
                  ) : (
                    checkout.familyMembers.map((m, i) => (
                      <div className="family-row" key={i}>
                        <input
                          placeholder="Full name"
                          value={m.fullName}
                          onChange={(e) =>
                            dispatch(updateFamilyMember({ index: i, patch: { fullName: e.target.value } }))
                          }
                        />
                        <input
                          placeholder="Relationship"
                          value={m.relationship || ''}
                          onChange={(e) =>
                            dispatch(updateFamilyMember({ index: i, patch: { relationship: e.target.value } }))
                          }
                        />
                        <button type="button" className="linkish" onClick={() => dispatch(removeFamilyMember(i))}>
                          Remove
                        </button>
                      </div>
                    ))
                  )}
                </div>
              )}

              <div className="form-actions end">
                <Button type="button" variant="dark" icon={<ArrowRight size={16} />} onClick={onNextPersonal}>
                  Next
                </Button>
              </div>
            </div>
          )}

          {checkout.step === 2 && (
            <div className="form-block">
              <h3>Chronic Diseases (Optional)</h3>
              <div className="disease-grid">
                {diseases.map((d) => (
                  <label key={d.id} className="check-item">
                    <input
                      type="checkbox"
                      checked={checkout.chronicDiseaseIds.includes(d.id)}
                      onChange={() => dispatch(toggleDisease(d.id))}
                    />
                    <span>{d.name}</span>
                  </label>
                ))}
              </div>
              <label>
                Additional Details or Notes (Optional)
                <textarea
                  rows={4}
                  value={checkout.medicalNotes}
                  onChange={(e) => dispatch(patchCheckout({ medicalNotes: e.target.value }))}
                  placeholder="Add any additional details about your medical conditions (optional)"
                />
              </label>
              <p className="help">This information helps us provide better personalized care. You can skip this step if you prefer.</p>
              <div className="privacy-box">
                Privacy Notice: Your information is confidential and will only be used to improve your healthcare
                experience.
              </div>
              <div className="form-actions">
                <Button type="button" variant="outline" icon={<ArrowLeft size={16} />} onClick={() => dispatch(setStep(1))}>
                  Previous
                </Button>
                <Button type="button" variant="dark" icon={<ArrowRight size={16} />} onClick={onNextMedical}>
                  Next
                </Button>
              </div>
            </div>
          )}

          {checkout.step === 3 && (
            <div className="form-block">
              <h3>Delivery & Payment</h3>
              <h4>Delivery Address</h4>
              <div className="grid-2">
                <label>
                  Country *
                  <input value={checkout.country} onChange={(e) => dispatch(patchCheckout({ country: e.target.value }))} />
                </label>
                <label>
                  Governorate *
                  <select
                    value={checkout.governorate}
                    onChange={(e) => dispatch(patchCheckout({ governorate: e.target.value }))}
                  >
                    <option value="">Select governorate</option>
                    {governorates.map((g) => (
                      <option key={g.id} value={g.name}>
                        {g.name}
                      </option>
                    ))}
                  </select>
                </label>
              </div>
              <label>
                Postal Code *
                <input
                  value={checkout.postalCode}
                  onChange={(e) => dispatch(patchCheckout({ postalCode: e.target.value }))}
                />
              </label>
              <label>
                Address Details *
                <textarea
                  rows={3}
                  value={checkout.addressDetails}
                  onChange={(e) => dispatch(patchCheckout({ addressDetails: e.target.value }))}
                />
              </label>

              <h4>Payment Information</h4>
              <div className="pay-tabs">
                {(paymentConfig?.methods ?? [
                  { method: 'CreditCard', label: 'Credit Card', available: true },
                  { method: 'DigitalWallet', label: 'Digital Wallet', available: false },
                  { method: 'BankTransfer', label: 'Bank Transfer', available: false },
                ]).map((m) => (
                  <button
                    key={m.method}
                    type="button"
                    disabled={!m.available}
                    title={m.unavailableReason ?? undefined}
                    className={checkout.paymentMethod === m.method ? 'is-active' : ''}
                    onClick={() => {
                      if (!m.available) {
                        setError(m.unavailableReason ?? `${m.label} is not configured in Paymob yet.`);
                        return;
                      }
                      setError(null);
                      dispatch(
                        patchCheckout({
                          paymentMethod: m.method as 'CreditCard' | 'DigitalWallet' | 'BankTransfer',
                        }),
                      );
                    }}
                  >
                    {m.label}
                    {!m.available ? ' (soon)' : ''}
                  </button>
                ))}
              </div>
              <p className="help">
                Selected method opens Paymob Unified Checkout. Wallet / Bank Transfer need their own
                Integration IDs in your Paymob dashboard.
              </p>

              <div className="order-mini">
                <div>
                  <span>Plan Selected</span>
                  <strong>{checkout.planName}</strong>
                </div>
                <div>
                  <span>Plan Type</span>
                  <strong>{checkout.planType}</strong>
                </div>
                <div>
                  <span>Billing Cycle</span>
                  <strong>{checkout.billingCycle}</strong>
                </div>
                <div>
                  <span>Total Amount</span>
                  <strong className="total">E£{checkout.priceEgp.toLocaleString()}</strong>
                </div>
              </div>

              <div className="form-actions">
                <Button type="button" variant="outline" icon={<ArrowLeft size={16} />} onClick={() => dispatch(setStep(2))}>
                  Previous
                </Button>
                <Button
                  type="button"
                  variant="dark"
                  icon={<Check size={16} />}
                  disabled={starting || paying}
                  onClick={onPay}
                >
                  {starting || paying ? 'Processing…' : 'Complete Subscription'}
                </Button>
              </div>
            </div>
          )}
        </section>
      </div>
    </div>
  );
}
