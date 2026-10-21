import {
  Award,
  Crown,
  Flame,
  Gift,
  Heart,
  Medal,
  Pill,
  Shield,
  Sparkles,
  Star,
  Stethoscope,
  Zap,
  type LucideIcon,
} from 'lucide-react';
import type { ProgramIcon } from '../types/api';

const map: Record<ProgramIcon, LucideIcon> = {
  Crown,
  Star,
  Medal,
  Shield,
  Heart,
  Stethoscope,
  Pill,
  MedicalCross: Shield,
  Sparkles,
  Award,
  Gift,
  Zap,
  Flame,
};

export function ProgramIconView({
  name,
  size = 20,
  color = 'currentColor',
}: {
  name: string;
  size?: number;
  color?: string;
}) {
  const Icon = map[name as ProgramIcon] ?? Shield;
  return <Icon size={size} color={color} />;
}

export function tierGradient(color: string) {
  switch (color) {
    case 'Purple':
      return 'var(--tier-platinum)';
    case 'Gold':
      return 'var(--tier-gold)';
    case 'Silver':
      return 'var(--tier-silver)';
    case 'Bronze':
      return 'var(--tier-bronze)';
    default:
      return 'var(--tier-gold)';
  }
}
