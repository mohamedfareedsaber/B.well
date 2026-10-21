import { Check, CreditCard, Heart, MapPin, User } from 'lucide-react';
import './Stepper.css';

const steps = [
  { key: 1, label: 'Personal Info', Icon: User },
  { key: 2, label: 'Medical Info', Icon: Heart },
  { key: 3, label: 'Delivery & Payment', Icon: MapPin },
] as const;

export function CheckoutStepper({
  step,
  mode = 'progress',
}: {
  step: 1 | 2 | 3 | 4;
  mode?: 'progress' | 'success' | 'failed';
}) {
  return (
    <div className={`stepper stepper--${mode}`}>
      <div className="stepper__bars">
        {steps.map((s) => (
          <div
            key={s.key}
            className={`stepper__bar ${
              mode === 'success' || step > s.key || (mode !== 'failed' && step === s.key) ? 'is-on' : ''
            } ${mode === 'failed' ? 'is-fail' : ''}`}
          />
        ))}
      </div>
      <div className="stepper__labels">
        {steps.map((s) => {
          const done = mode === 'success' || step > s.key;
          const active = step === s.key && mode === 'progress';
          const failed = mode === 'failed';
          return (
            <div
              key={s.key}
              className={`stepper__item ${done ? 'is-done' : ''} ${active ? 'is-active' : ''} ${
                failed ? 'is-failed' : ''
              }`}
            >
              <span className="stepper__icon">
                {failed ? '×' : done ? <Check size={14} /> : <s.Icon size={14} />}
              </span>
              <span>{s.label}</span>
            </div>
          );
        })}
      </div>
      {mode === 'progress' && step === 3 && (
        <div className="stepper__pay-hint">
          <CreditCard size={14} /> Payment
        </div>
      )}
    </div>
  );
}
