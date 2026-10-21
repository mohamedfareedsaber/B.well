import { useMemo, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { Search, Users, Stethoscope, X } from 'lucide-react';
import { AdminHeader } from '../components/Headers';
import { useAppSelector } from '../app/hooks';
import {
  useGetPatientQuery,
  useGetPatientsOverviewQuery,
  useGetPatientsQuery,
} from '../services/bwellApi';
import './AdminPages.css';

export function AdminPatientsPage() {
  const token = useAppSelector((s) => s.auth.token);
  const [q, setQ] = useState('');
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: overview } = useGetPatientsOverviewQuery(undefined, { skip: !token });
  const { data: patients = [], isLoading } = useGetPatientsQuery(q || undefined, { skip: !token });
  const { data: details } = useGetPatientQuery(selectedId!, { skip: !token || !selectedId });

  const sorted = useMemo(
    () =>
      [...patients].sort(
        (a, b) => new Date(b.subscribedAtUtc).getTime() - new Date(a.subscribedAtUtc).getTime(),
      ),
    [patients],
  );

  if (!token) return <Navigate to="/login" replace />;

  return (
    <div className="admin-page">
      <AdminHeader active="patients" />
      <main className="container-wide admin-main">
        <div className="kpi-grid kpi-grid--2">
          <article className="kpi">
            <div className="kpi-icon">
              <Users size={20} />
            </div>
            <div>
              <p>Total Patients</p>
              <h2>{overview?.totalPatients ?? '—'}</h2>
              <span>Registered members</span>
            </div>
          </article>
          <article className="kpi">
            <div className="kpi-icon">
              <Stethoscope size={20} />
            </div>
            <div>
              <p>Total Visits</p>
              <h2>{overview?.totalVisits ?? '—'}</h2>
              <span>Recorded clinic visits</span>
            </div>
          </article>
        </div>

        <section className="panel">
          <div className="panel__head">
            <div>
              <h3>Patients Management</h3>
              <p className="muted">Search and review subscribed patients.</p>
            </div>
            <label className="search-field">
              <Search size={16} />
              <input
                value={q}
                onChange={(e) => setQ(e.target.value)}
                placeholder="Search by name, mobile, or code…"
              />
            </label>
          </div>

          {isLoading && <p className="state">Loading patients…</p>}

          <div className="table-wrap">
            <table className="patients-table">
              <thead>
                <tr>
                  <th>Patient</th>
                  <th>Plan</th>
                  <th>Mobile</th>
                  <th>Visits</th>
                  <th>Conditions</th>
                  <th>Subscribed</th>
                </tr>
              </thead>
              <tbody>
                {sorted.map((p) => (
                  <tr key={p.id} onClick={() => setSelectedId(p.id)}>
                    <td>
                      <strong>{p.fullName}</strong>
                      <small>{p.patientCode}</small>
                    </td>
                    <td>
                      {p.planName ? (
                        <span className="pill yellow">
                          {p.planName} {p.planType ? `· ${p.planType}` : ''}
                        </span>
                      ) : (
                        <span className="muted">—</span>
                      )}
                    </td>
                    <td>{p.mobileNumber}</td>
                    <td>{p.visitCount}</td>
                    <td>
                      {p.chronicDiseases.length
                        ? p.chronicDiseases.slice(0, 2).join(', ')
                        : '—'}
                      {p.chronicDiseases.length > 2 ? '…' : ''}
                    </td>
                    <td>{new Date(p.subscribedAtUtc).toLocaleDateString()}</td>
                  </tr>
                ))}
                {!isLoading && !sorted.length && (
                  <tr>
                    <td colSpan={6} className="muted center">
                      No patients found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </section>
      </main>

      {selectedId && (
        <div className="drawer-backdrop" onClick={() => setSelectedId(null)}>
          <aside className="patient-drawer" onClick={(e) => e.stopPropagation()}>
            <div className="drawer-head">
              <h3>Patient Details</h3>
              <button type="button" onClick={() => setSelectedId(null)} aria-label="Close">
                <X size={18} />
              </button>
            </div>
            {!details && <p className="state">Loading…</p>}
            {details && (
              <div className="drawer-body">
                <div className="drawer-block">
                  <h4>{details.fullName}</h4>
                  <p className="muted">{details.patientCode}</p>
                  <div className="detail-grid">
                    <span>Mobile</span>
                    <strong>{details.mobileNumber}</strong>
                    <span>Email</span>
                    <strong>{details.email || '—'}</strong>
                    <span>Age</span>
                    <strong>{details.age ?? '—'}</strong>
                    <span>Nationality</span>
                    <strong>{details.nationality || '—'}</strong>
                    <span>ID / Passport</span>
                    <strong>{details.nationalIdOrPassport || '—'}</strong>
                  </div>
                </div>

                <div className="drawer-block">
                  <h5>Subscription</h5>
                  <div className="detail-grid">
                    <span>Plan</span>
                    <strong>
                      {details.activePlanName || '—'}
                      {details.activePlanType ? ` · ${details.activePlanType}` : ''}
                    </strong>
                    <span>Status</span>
                    <strong>{details.subscriptionStatus || '—'}</strong>
                    <span>Next renewal</span>
                    <strong>
                      {details.nextRenewalDateUtc
                        ? new Date(details.nextRenewalDateUtc).toLocaleDateString()
                        : '—'}
                    </strong>
                  </div>
                </div>

                <div className="drawer-block">
                  <h5>Chronic Diseases</h5>
                  {details.chronicDiseases.length ? (
                    <div className="pill-row">
                      {details.chronicDiseases.map((d) => (
                        <span key={d} className="pill blue">
                          {d}
                        </span>
                      ))}
                    </div>
                  ) : (
                    <p className="muted">None recorded</p>
                  )}
                  {details.medicalNotes && <p className="notes">{details.medicalNotes}</p>}
                </div>

                {!!details.familyMembers.length && (
                  <div className="drawer-block">
                    <h5>Family Members</h5>
                    <ul className="simple-list">
                      {details.familyMembers.map((m) => (
                        <li key={m.id}>
                          <strong>{m.fullName}</strong>
                          <span>
                            {[m.relationship, m.age != null ? `Age ${m.age}` : null]
                              .filter(Boolean)
                              .join(' · ')}
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}

                {details.deliveryAddress && (
                  <div className="drawer-block">
                    <h5>Delivery Address</h5>
                    <p>
                      {details.deliveryAddress.addressDetails}
                      <br />
                      {details.deliveryAddress.governorate}, {details.deliveryAddress.country}{' '}
                      {details.deliveryAddress.postalCode}
                    </p>
                  </div>
                )}

                <div className="drawer-block">
                  <h5>Visit History</h5>
                  {details.visits.length ? (
                    <ul className="simple-list">
                      {details.visits.map((v) => (
                        <li key={v.id}>
                          <strong>{v.medicalServiceName || 'Visit'}</strong>
                          <span>
                            {new Date(v.visitDateUtc).toLocaleDateString()} · Seq{' '}
                            {v.visitSequenceForService}
                            {v.discountAppliedPercent != null
                              ? ` · ${v.discountAppliedPercent}% off`
                              : ''}
                          </span>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <p className="muted">No visits yet</p>
                  )}
                </div>
              </div>
            )}
          </aside>
        </div>
      )}
    </div>
  );
}
