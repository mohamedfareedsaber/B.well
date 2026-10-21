import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { LogIn } from 'lucide-react';
import { BrandLogo } from '../components/BrandLogo';
import { Button } from '../components/Button';
import { useLoginMutation } from '../services/bwellApi';
import { useAppDispatch } from '../app/hooks';
import { setCredentials } from '../features/auth/authSlice';
import { getErrorMessage } from '../utils/errors';
import './LoginPage.css';

export function LoginPage() {
  const [email, setEmail] = useState('admin@bwell.local');
  const [password, setPassword] = useState('Admin@123');
  const [login, { isLoading }] = useLoginMutation();
  const [error, setError] = useState<string | null>(null);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      const res = await login({ email, password }).unwrap();
      dispatch(setCredentials(res));
      navigate(res.role === 'Admin' ? '/admin' : '/portal');
    } catch (err) {
      setError(getErrorMessage(err, 'Invalid email or password.'));
    }
  };

  return (
    <div className="login-page">
      <form className="login-card" onSubmit={onSubmit}>
        <BrandLogo size="lg" />
        <h1>Sign In</h1>
        <p>Admin or Patient access to B.Well</p>
        {error && <div className="form-error">{error}</div>}
        <label>
          Email
          <input value={email} onChange={(e) => setEmail(e.target.value)} type="email" required />
        </label>
        <label>
          Password
          <input
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            type="password"
            required
            minLength={8}
          />
        </label>
        <Button type="submit" variant="dark" icon={<LogIn size={16} />} disabled={isLoading} className="full">
          {isLoading ? 'Signing in…' : 'Sign In'}
        </Button>
        <p className="login-hint">
          Patient demo: <code>omar@example.com</code> / <code>Patient@123</code>
        </p>
        <Link to="/" className="back-home">
          Back to Home
        </Link>
      </form>
    </div>
  );
}
