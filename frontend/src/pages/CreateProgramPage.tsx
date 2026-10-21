import { useMemo, useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { ArrowLeft, Plus, Trash2 } from 'lucide-react';
import { AdminHeader } from '../components/Headers';
import { Button } from '../components/Button';
import { ProgramIconView, tierGradient } from '../components/icons';
import { useAppSelector } from '../app/hooks';
import {
  useCreateProgramMutation,
  useGetMedicalServicesQuery,
} from '../services/bwellApi';
import type { ColorGradient, ProgramIcon } from '../types/api';
import './AdminPages.css';

const ICONS: ProgramIcon[] = [
  'Crown',
  'Star',
  'Medal',
  'Shield',
  'Heart',
  'Stethoscope',
  'Pill',
  'Sparkles',
  'Award',
  'Gift',
  'Zap',
  'Flame',
];

const COLORS: ColorGradient[] = ['Purple', 'Gold', 'Silver', 'Bronze', 'Blue', 'Green', 'Red'];

type DiscountRow = {
  medicalServiceId: string;
  firstVisitDiscountPercent: string;
  secondVisitDiscountPercent: string;
  additionalVisitDiscountPercent: string;
};

export function CreateProgramPage() {
  const token = useAppSelector((s) => s.auth.token);
  const navigate = useNavigate();
  const { data: services = [] } = useGetMedicalServicesQuery(undefined, { skip: !token });
  const [createProgram, { isLoading }] = useCreateProgramMutation();

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [icon, setIcon] = useState<ProgramIcon>('Crown');
  const [colorGradient, setColorGradient] = useState<ColorGradient>('Purple');
  const [individualPriceEgp, setIndividualPriceEgp] = useState('999');
  const [familyPriceEgp, setFamilyPriceEgp] = useState('1999');
  const [offerExpiryDate, setOfferExpiryDate] = useState('');
  const [individualBenefits, setIndividualBenefits] = useState<string[]>(['']);
  const [familyBenefits, setFamilyBenefits] = useState<string[]>(['']);
  const [discounts, setDiscounts] = useState<DiscountRow[]>([]);
  const [error, setError] = useState<string | null>(null);

  const previewBenefits = useMemo(
    () => individualBenefits.filter((b) => b.trim()).slice(0, 4),
    [individualBenefits],
  );

  if (!token) return <Navigate to="/login" replace />;

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    const ind = individualBenefits.map((b) => b.trim()).filter(Boolean);
    const fam = familyBenefits.map((b) => b.trim()).filter(Boolean);
    if (!name.trim() || !description.trim()) {
      setError('Name and description are required.');
      return;
    }
    if (!ind.length) {
      setError('Add at least one individual benefit.');
      return;
    }

    try {
      await createProgram({
        name: name.trim(),
        description: description.trim(),
        icon,
        colorGradient,
        individualPriceEgp: Number(individualPriceEgp),
        familyPriceEgp: Number(familyPriceEgp),
        offerExpiryDate: offerExpiryDate || null,
        individualBenefits: ind,
        familyBenefits: fam.length ? fam : ind,
        serviceDiscounts: discounts
          .filter((d) => d.medicalServiceId)
          .map((d) => ({
            medicalServiceId: d.medicalServiceId,
            firstVisitDiscountPercent: Number(d.firstVisitDiscountPercent || 0),
            secondVisitDiscountPercent: Number(d.secondVisitDiscountPercent || 0),
            additionalVisitDiscountPercent: Number(d.additionalVisitDiscountPercent || 0),
          })),
      }).unwrap();
      navigate('/admin/programs');
    } catch {
      setError('Failed to create program. Check fields and try again.');
    }
  };

  return (
    <div className="admin-page">
      <AdminHeader active="programs" />
      <main className="container-wide admin-main">
        <div className="create-layout">
          <form className="panel create-form" onSubmit={onSubmit}>
            <div className="panel__head">
              <div>
                <Link to="/admin/programs" className="text-link">
                  <ArrowLeft size={16} /> Back to Programs
                </Link>
                <h3>Create New Program</h3>
                <p className="muted">Define pricing, benefits, and visit discounts.</p>
              </div>
            </div>

            {error && <div className="form-error">{error}</div>}

            <div className="form-grid-2">
              <label>
                Program Name
                <input value={name} onChange={(e) => setName(e.target.value)} required />
              </label>
              <label>
                Offer Expiry Date
                <input
                  type="date"
                  value={offerExpiryDate}
                  onChange={(e) => setOfferExpiryDate(e.target.value)}
                />
              </label>
            </div>

            <label>
              Description
              <textarea
                rows={3}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                required
              />
            </label>

            <div className="form-grid-2">
              <label>
                Individual Price (EGP)
                <input
                  type="number"
                  min={0}
                  value={individualPriceEgp}
                  onChange={(e) => setIndividualPriceEgp(e.target.value)}
                  required
                />
              </label>
              <label>
                Family Price (EGP)
                <input
                  type="number"
                  min={0}
                  value={familyPriceEgp}
                  onChange={(e) => setFamilyPriceEgp(e.target.value)}
                  required
                />
              </label>
            </div>

            <fieldset>
              <legend>Icon</legend>
              <div className="icon-picker">
                {ICONS.map((i) => (
                  <button
                    key={i}
                    type="button"
                    className={icon === i ? 'is-active' : ''}
                    onClick={() => setIcon(i)}
                  >
                    <ProgramIconView name={i} size={18} />
                  </button>
                ))}
              </div>
            </fieldset>

            <fieldset>
              <legend>Color Gradient</legend>
              <div className="color-picker">
                {COLORS.map((c) => (
                  <button
                    key={c}
                    type="button"
                    className={colorGradient === c ? 'is-active' : ''}
                    style={{ background: tierGradient(c) }}
                    onClick={() => setColorGradient(c)}
                    title={c}
                  />
                ))}
              </div>
            </fieldset>

            <BenefitEditor
              title="Individual Benefits"
              values={individualBenefits}
              onChange={setIndividualBenefits}
            />
            <BenefitEditor
              title="Family Benefits"
              values={familyBenefits}
              onChange={setFamilyBenefits}
            />

            <div className="discount-block">
              <div className="panel__head">
                <h4>Service Visit Discounts</h4>
                <Button
                  type="button"
                  variant="outline"
                  icon={<Plus size={14} />}
                  onClick={() =>
                    setDiscounts((d) => [
                      ...d,
                      {
                        medicalServiceId: services[0]?.id ?? '',
                        firstVisitDiscountPercent: '20',
                        secondVisitDiscountPercent: '10',
                        additionalVisitDiscountPercent: '5',
                      },
                    ])
                  }
                >
                  Add Discount
                </Button>
              </div>
              {discounts.map((row, idx) => (
                <div key={idx} className="discount-row">
                  <select
                    value={row.medicalServiceId}
                    onChange={(e) => {
                      const next = [...discounts];
                      next[idx] = { ...row, medicalServiceId: e.target.value };
                      setDiscounts(next);
                    }}
                  >
                    <option value="">Select service</option>
                    {services.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.name}
                      </option>
                    ))}
                  </select>
                  <input
                    type="number"
                    placeholder="1st %"
                    value={row.firstVisitDiscountPercent}
                    onChange={(e) => {
                      const next = [...discounts];
                      next[idx] = { ...row, firstVisitDiscountPercent: e.target.value };
                      setDiscounts(next);
                    }}
                  />
                  <input
                    type="number"
                    placeholder="2nd %"
                    value={row.secondVisitDiscountPercent}
                    onChange={(e) => {
                      const next = [...discounts];
                      next[idx] = { ...row, secondVisitDiscountPercent: e.target.value };
                      setDiscounts(next);
                    }}
                  />
                  <input
                    type="number"
                    placeholder="Addl %"
                    value={row.additionalVisitDiscountPercent}
                    onChange={(e) => {
                      const next = [...discounts];
                      next[idx] = { ...row, additionalVisitDiscountPercent: e.target.value };
                      setDiscounts(next);
                    }}
                  />
                  <button
                    type="button"
                    className="icon-btn"
                    onClick={() => setDiscounts((d) => d.filter((_, i) => i !== idx))}
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              ))}
            </div>

            <div className="form-actions">
              <Link to="/admin/programs">
                <Button type="button" variant="outline">
                  Cancel
                </Button>
              </Link>
              <Button type="submit" variant="dark" disabled={isLoading}>
                {isLoading ? 'Creating…' : 'Create Program'}
              </Button>
            </div>
          </form>

          <aside className="panel preview-card">
            <p className="muted">Live Preview</p>
            <div className="preview-tier" style={{ background: tierGradient(colorGradient) }}>
              <ProgramIconView name={icon} color="#fff" size={28} />
              <h3>{name || 'Program Name'}</h3>
              <p>{description || 'Short description appears here.'}</p>
            </div>
            <div className="preview-prices">
              <span>Individual EGP {individualPriceEgp || '0'}</span>
              <span>Family EGP {familyPriceEgp || '0'}</span>
            </div>
            <ul>
              {previewBenefits.length ? (
                previewBenefits.map((b) => <li key={b}>{b}</li>)
              ) : (
                <li className="muted">Benefits will show here</li>
              )}
            </ul>
          </aside>
        </div>
      </main>
    </div>
  );
}

function BenefitEditor({
  title,
  values,
  onChange,
}: {
  title: string;
  values: string[];
  onChange: (v: string[]) => void;
}) {
  return (
    <div className="benefit-editor">
      <div className="panel__head">
        <h4>{title}</h4>
        <Button
          type="button"
          variant="outline"
          icon={<Plus size={14} />}
          onClick={() => onChange([...values, ''])}
        >
          Add
        </Button>
      </div>
      {values.map((v, i) => (
        <div key={i} className="benefit-row">
          <input
            value={v}
            onChange={(e) => {
              const next = [...values];
              next[i] = e.target.value;
              onChange(next);
            }}
            placeholder={`Benefit ${i + 1}`}
          />
          <button
            type="button"
            className="icon-btn"
            onClick={() => onChange(values.filter((_, idx) => idx !== i))}
            disabled={values.length === 1}
          >
            <Trash2 size={16} />
          </button>
        </div>
      ))}
    </div>
  );
}
