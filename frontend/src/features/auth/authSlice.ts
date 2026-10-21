import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { LoginResponse } from '../../types/api';

type AuthState = {
  token: string | null;
  fullName: string | null;
  email: string | null;
  role: string | null;
  patientId: string | null;
};

const saved = localStorage.getItem('bwell_auth');
const initial: AuthState = saved
  ? (JSON.parse(saved) as AuthState)
  : { token: null, fullName: null, email: null, role: null, patientId: null };

const authSlice = createSlice({
  name: 'auth',
  initialState: initial,
  reducers: {
    setCredentials: (state, action: PayloadAction<LoginResponse>) => {
      state.token = action.payload.token;
      state.fullName = action.payload.fullName;
      state.email = action.payload.email;
      state.role = action.payload.role;
      state.patientId = action.payload.patientId ?? null;
      localStorage.setItem('bwell_auth', JSON.stringify(state));
    },
    logout: (state) => {
      state.token = null;
      state.fullName = null;
      state.email = null;
      state.role = null;
      state.patientId = null;
      localStorage.removeItem('bwell_auth');
    },
  },
});

export const { setCredentials, logout } = authSlice.actions;
export default authSlice.reducer;
