import { Link, Navigate, useParams, useSearchParams } from 'react-router-dom';
import { Download, Home, Check } from 'lucide-react';
import { PublicHeader } from '../components/Headers';
import { CheckoutStepper } from '../components/Stepper';
import { Button } from '../components/Button';
import { useAppSelector } from '../app/hooks';
import { useGetOrderSummaryQuery } from '../services/bwellApi';
import './ResultPages.css';

export function PaymentResultPage() {
  const [params] = useSearchParams();
  const checkoutSubId = useAppSelector((s) => s.checkout.subscriptionId);
  const successFlag = params.get('success') ?? params.get('txn_response_code');
  const merchantOrder =
    params.get('merchant_order_id') ??
    params.get('special_reference') ??
    params.get('subscription_id') ??
    checkoutSubId;

  const isSuccess =
    successFlag === 'true' ||
    successFlag === 'True' ||
    successFlag === 'APPROVED' ||
    successFlag === '00' ||
    (!successFlag && !!merchantOrder);

  if (!merchantOrder) {
    return <Navigate to={isSuccess ? '/checkout/success/' : '/checkout/failed'} replace />;
  }

  return (
    <Navigate
      to={isSuccess ? `/checkout/success/${merchantOrder}` : `/checkout/failed/${merchantOrder}`}
      replace
    />
  );
}

export function SuccessPage() {
  const { id = '' } = useParams();
  const { data, isLoading } = useGetOrderSummaryQuery(id, { skip: !id });

  return (
    <div className="result-page">
      <PublicHeader />
      <main className="container result-card">
        <CheckoutStepper step={4} mode="success" />
        <div className="result-icon success">
          <Check size={36} />
        </div>
        <h1>Subscription Completed Successfully</h1>
        <p>
          Thank you! Your payment has been processed and your {data?.planName} {data?.planType} plan is
          now active.
        </p>

        {isLoading && <p className="state">Loading order…</p>}
        {data && (
          <div className="summary-box">
            <h3>Order Summary</h3>
            <div>
              <span>Plan</span>
              <strong>
                {data.planName} – {data.planType}
              </strong>
            </div>
            <div>
              <span>Billing Cycle</span>
              <strong>{data.billingCycle}</strong>
            </div>
            <div>
              <span>Total Paid</span>
              <strong className="yellow">E£{data.totalPaidEgp.toLocaleString()}</strong>
            </div>
            <div>
              <span>Next Renewal Date</span>
              <strong>
                {data.nextRenewalDateUtc
                  ? new Date(data.nextRenewalDateUtc).toLocaleDateString(undefined, {
                      day: '2-digit',
                      month: 'long',
                      year: 'numeric',
                    })
                  : '—'}
              </strong>
            </div>
          </div>
        )}

        <Link to="/portal">
          <Button variant="dark" icon={<Home size={16} />} className="full">
            Go to My Portal
          </Button>
        </Link>
        <button type="button" className="invoice-link">
          <Download size={16} /> Download Invoice
        </button>
      </main>
    </div>
  );
}

export function FailedPage() {
  const { id = '' } = useParams();
  const { data } = useGetOrderSummaryQuery(id, { skip: !id });

  return (
    <div className="result-page">
      <PublicHeader />
      <main className="container result-card">
        <CheckoutStepper step={3} mode="failed" />
        <div className="result-icon fail">×</div>
        <h1 className="fail-title">Payment Failed</h1>
        <p>
          Unfortunately, we couldn&apos;t process your payment. This could be due to insufficient funds,
          incorrect card details, or a network issue.
        </p>

        <div className="summary-box fail-box">
          <h3>Transaction Details</h3>
          <div>
            <span>Plan</span>
            <strong>
              {data?.planName ?? '—'} – {data?.planType ?? '—'}
            </strong>
          </div>
          <div>
            <span>Amount</span>
            <strong className="fail-amount">E£{(data?.totalPaidEgp ?? 0).toLocaleString()}</strong>
          </div>
          <div>
            <span>Status</span>
            <strong className="badge-fail">Failed</strong>
          </div>
          <div>
            <span>Payment Method</span>
            <strong>{data?.paymentMethod ?? 'Credit Card'}</strong>
          </div>
        </div>

        <div className="solutions-box">
          <h3>Common Solutions</h3>
          <ul>
            <li>Check your card details and expiry date</li>
            <li>Ensure you have sufficient funds</li>
            <li>Try a different payment method</li>
            <li>Contact your bank if the issue persists</li>
          </ul>
        </div>

        <div className="result-actions">
          <Link to="/checkout">
            <Button variant="danger">Try Again</Button>
          </Link>
          <Link to="/">
            <Button variant="outline" icon={<Home size={16} />}>
              Back to Home
            </Button>
          </Link>
        </div>
      </main>
    </div>
  );
}
