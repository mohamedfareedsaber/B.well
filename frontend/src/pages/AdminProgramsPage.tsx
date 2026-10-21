import { Link, Navigate } from 'react-router-dom';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { AdminHeader } from '../components/Headers';
import { Button } from '../components/Button';
import { ProgramIconView, tierGradient } from '../components/icons';
import { useAppSelector } from '../app/hooks';
import { useDeleteProgramMutation, useGetProgramsQuery } from '../services/bwellApi';
import './AdminPages.css';

export function AdminProgramsPage() {
  const token = useAppSelector((s) => s.auth.token);
  const { data = [], isLoading } = useGetProgramsQuery(undefined, { skip: !token });
  const [remove] = useDeleteProgramMutation();

  if (!token) return <Navigate to="/login" replace />;

  return (
    <div className="admin-page">
      <AdminHeader active="programs" />
      <main className="container-wide admin-main">
        <div className="panel">
          <div className="panel__head">
            <div>
              <h3>Manage Programs</h3>
              <p className="muted">View and manage all active loyalty programs.</p>
            </div>
            <Link to="/admin/programs/new">
              <Button variant="dark" icon={<Plus size={16} />}>
                Create New Program
              </Button>
            </Link>
          </div>

          {isLoading && <p className="state">Loading…</p>}
          <div className="programs-admin-grid">
            {data.map((p) => (
              <article key={p.id} className="admin-program-card">
                <div className="admin-program-card__top">
                  <div className="icon-sq" style={{ background: tierGradient(p.colorGradient) }}>
                    <ProgramIconView name={p.icon} color="#fff" />
                  </div>
                  <div className="grow">
                    <h4>{p.name} Plan</h4>
                    <p>{p.description}</p>
                  </div>
                  <div className="row-actions">
                    <button type="button" title="Edit" disabled>
                      <Pencil size={16} color="var(--bw-yellow-dark)" />
                    </button>
                    <button
                      type="button"
                      title="Delete"
                      onClick={() => {
                        if (confirm(`Delete ${p.name}?`)) void remove(p.id);
                      }}
                    >
                      <Trash2 size={16} color="var(--bw-danger)" />
                    </button>
                  </div>
                </div>
                <div className="pill-row">
                  <span className="pill blue">Individual EGP {p.individualPriceEgp}</span>
                  <span className="pill purple">Family EGP {p.familyPriceEgp}</span>
                  <span className="pill green">{p.memberCount} members</span>
                  <span className="pill yellow">EGP {p.revenueEgp.toLocaleString()}</span>
                </div>
                <ul>
                  {p.benefits
                    .filter((b) => b.appliesTo === 'Individual')
                    .slice(0, 3)
                    .map((b) => (
                      <li key={b.id}>{b.text}</li>
                    ))}
                </ul>
                <button type="button" className="more-link">
                  + more benefits
                </button>
              </article>
            ))}
          </div>
        </div>
      </main>
    </div>
  );
}
