import { Link, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  BarChart3,
  ChevronDown,
  Eye,
  FileText,
  LogIn,
  LogOut,
  User,
  Users,
} from 'lucide-react';
import { BrandLogo } from './BrandLogo';
import { Button } from './Button';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { logout } from '../features/auth/authSlice';
import './Headers.css';

export function PublicHeader({ showBackToPlans = false }: { showBackToPlans?: boolean }) {
  const token = useAppSelector((s) => s.auth.token);
  const role = useAppSelector((s) => s.auth.role);
  const dashboardPath = role === 'Admin' ? '/admin' : '/portal';
  return (
    <header className="pub-header">
      <div className="container-wide pub-header__inner">
        <Link to="/">
          <BrandLogo />
        </Link>
        <div className="pub-header__actions">
          {showBackToPlans && (
            <Link className="text-link" to="/plans">
              <ArrowLeft size={16} /> Back to Plans
            </Link>
          )}
          {token ? (
            <Link to={dashboardPath}>
              <Button variant="dark" icon={<User size={16} />}>
                Dashboard
              </Button>
            </Link>
          ) : (
            <Link to="/login">
              <Button variant="dark" icon={<LogIn size={16} />}>
                Sign In
              </Button>
            </Link>
          )}
        </div>
      </div>
    </header>
  );
}

export function AdminHeader({
  active,
}: {
  active: 'overview' | 'programs' | 'patients';
}) {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const fullName = useAppSelector((s) => s.auth.fullName) ?? 'Admin User';

  return (
    <header className="admin-header">
      <div className="container-wide admin-header__top">
        <Link to="/admin">
          <BrandLogo />
        </Link>
        <div className="admin-header__actions">
          <Link to="/plans">
            <Button variant="outline" icon={<Eye size={16} />}>
              Preview Programs
            </Button>
          </Link>
          <div className="admin-user">
            <span className="admin-user__avatar">
              <User size={14} />
            </span>
            <span>{fullName}</span>
            <ChevronDown size={14} color="#9ca3af" />
          </div>
          <Button
            variant="ghost"
            icon={<LogOut size={16} />}
            onClick={() => {
              dispatch(logout());
              navigate('/login');
            }}
          >
            Logout
          </Button>
        </div>
      </div>

      <div className="container-wide admin-header__title-row">
        <div>
          <h1>Admin Dashboard</h1>
          <p>Manage loyalty programs and view statistics.</p>
        </div>
        <nav className="admin-tabs">
          <Link className={active === 'overview' ? 'is-active' : ''} to="/admin">
            <BarChart3 size={16} /> Overview
          </Link>
          <Link className={active === 'programs' ? 'is-active' : ''} to="/admin/programs">
            <FileText size={16} /> Manage Programs
          </Link>
          <Link className={active === 'patients' ? 'is-active' : ''} to="/admin/patients">
            <Users size={16} /> Patients Management
          </Link>
        </nav>
      </div>
    </header>
  );
}
