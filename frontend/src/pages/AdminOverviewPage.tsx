import { Navigate } from 'react-router-dom';
import { Cell, Pie, PieChart, ResponsiveContainer } from 'recharts';
import { DollarSign, Filter, TrendingUp, Users, Zap } from 'lucide-react';
import { AdminHeader } from '../components/Headers';
import { ProgramIconView } from '../components/icons';
import { EgyptMap } from '../components/EgyptMap';
import { useAppSelector } from '../app/hooks';
import { useGetDashboardQuery } from '../services/bwellApi';
import './AdminPages.css';

const diseaseColors = ['#ef4444', '#3b82f6', '#4b5563', '#a855f7', '#ec4899', '#f97316'];

export function AdminOverviewPage() {
  const token = useAppSelector((s) => s.auth.token);
  const { data, isLoading, isError } = useGetDashboardQuery(undefined, { skip: !token });

  if (!token) return <Navigate to="/login" replace />;

  return (
    <div className="admin-page">
      <AdminHeader active="overview" />
      <main className="container-wide admin-main">
        {isLoading && <p className="state">Loading dashboard…</p>}
        {isError && <p className="state error">Failed to load dashboard.</p>}
        {data && (
          <>
            <div className="kpi-grid">
              <article className="kpi kpi--dark">
                <div className="kpi-icon kpi-icon--yellow">
                  <DollarSign size={20} />
                </div>
                <div>
                  <p>Total Revenue</p>
                  <h2>E£{Math.round(data.totalRevenueEgp).toLocaleString()}</h2>
                  <span>Annual recurring revenue</span>
                </div>
              </article>
              <article className="kpi">
                <div className="kpi-icon">
                  <Users size={20} />
                </div>
                <div>
                  <p>Total Subscriptions</p>
                  <h2>{data.totalSubscriptions.toLocaleString()}</h2>
                  <span>Active members across all programs</span>
                </div>
              </article>
              <article className="kpi">
                <div className="kpi-icon">
                  <Zap size={20} />
                </div>
                <div>
                  <p>Active Programs</p>
                  <h2>{data.activePrograms}</h2>
                  <span>Live loyalty programs</span>
                </div>
              </article>
              <article className="kpi">
                <div className="kpi-icon">
                  <TrendingUp size={20} />
                </div>
                <div>
                  <p>Avg. Value</p>
                  <h2>E£{Math.round(data.averageValueEgp).toLocaleString()}</h2>
                  <span>Per member annually</span>
                </div>
              </article>
            </div>

            <div className="admin-two">
              <section className="panel">
                <div className="panel__head compact">
                  <div>
                    <h3>Program Performance</h3>
                    <p className="muted">Subscription and revenue breakdown by program</p>
                  </div>
                </div>
                <div className="perf-list">
                  {data.programPerformance.map((p) => (
                    <div key={p.programId} className="perf-item">
                      <div className="perf-item__top">
                        <div className="perf-item__name">
                          <span
                            className="perf-icon"
                            style={{ background: softColor(p.color), color: colorFor(p.color) }}
                          >
                            <ProgramIconView name={p.icon} size={16} color={colorFor(p.color)} />
                          </span>
                          <strong>{p.name}</strong>
                          <span className="chip">{p.memberCount} members</span>
                        </div>
                        <span className="perf-rev">
                          E£{Math.round(p.revenueEgp).toLocaleString()} revenue
                        </span>
                      </div>
                      <div className="bar-row">
                        <div className="bar">
                          <i
                            style={{
                              width: `${Math.max(p.sharePercent, 2)}%`,
                              background: colorFor(p.color),
                            }}
                          />
                        </div>
                        <small>{p.sharePercent.toFixed(1)}%</small>
                      </div>
                    </div>
                  ))}
                </div>
              </section>

              <section className="panel">
                <div className="panel__head compact">
                  <div>
                    <h3>Chronic Disease Statistics</h3>
                    <p className="muted">Health conditions prevalence among members</p>
                  </div>
                </div>
                <div className="disease-wrap">
                  <div className="donut">
                    <ResponsiveContainer width="100%" height={170}>
                      <PieChart>
                        <Pie
                          data={[
                            { name: 'With', value: data.chronicDiseaseStats.membersWithConditions },
                            {
                              name: 'Without',
                              value: Math.max(
                                0,
                                data.chronicDiseaseStats.totalMembers -
                                  data.chronicDiseaseStats.membersWithConditions,
                              ),
                            },
                          ]}
                          dataKey="value"
                          innerRadius={52}
                          outerRadius={74}
                          paddingAngle={2}
                          startAngle={90}
                          endAngle={-270}
                        >
                          <Cell fill="#3b82f6" />
                          <Cell fill="#e5e7eb" />
                        </Pie>
                      </PieChart>
                    </ResponsiveContainer>
                    <div className="donut-label">
                      <strong>{data.chronicDiseaseStats.percentWithConditions}%</strong>
                      <span>With Conditions</span>
                    </div>
                  </div>
                  <p className="muted center disease-count">
                    {data.chronicDiseaseStats.membersWithConditions} out of{' '}
                    {data.chronicDiseaseStats.totalMembers} members
                  </p>
                  <div className="disease-bars">
                    {data.chronicDiseaseStats.breakdown.map((b, i) => (
                      <div key={b.name} className="disease-row">
                        <div className="perf-item__top">
                          <span>{b.name}</span>
                          <span className="disease-meta">
                            {b.memberCount} ({b.percent.toFixed(1)}%)
                          </span>
                        </div>
                        <div className="bar">
                          <i
                            style={{
                              width: `${Math.max(b.percent, 2)}%`,
                              background: diseaseColors[i % diseaseColors.length],
                            }}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </section>
            </div>

            <section className="panel map-panel">
              <div className="panel__head">
                <div>
                  <h3>Egypt Map</h3>
                  <p className="muted">Geographic distribution visualization</p>
                </div>
                <div className="map-controls">
                  <button type="button" className="map-filter" aria-label="Filter">
                    <Filter size={16} />
                  </button>
                  <span className="chip egypt-chip">🇪🇬 Egypt ▾</span>
                </div>
              </div>
              <EgyptMap points={data.geographicDistribution} />
            </section>
          </>
        )}
      </main>
    </div>
  );
}

function colorFor(name: string) {
  switch (name) {
    case 'Purple':
      return '#8b5cf6';
    case 'Gold':
      return '#f59e0b';
    case 'Silver':
      return '#94a3b8';
    case 'Bronze':
      return '#b45309';
    default:
      return '#f0b90b';
  }
}

function softColor(name: string) {
  switch (name) {
    case 'Purple':
      return '#f3e8ff';
    case 'Gold':
      return '#fef3c7';
    case 'Silver':
      return '#f1f5f9';
    case 'Bronze':
      return '#ffedd5';
    default:
      return '#fff8e1';
  }
}
