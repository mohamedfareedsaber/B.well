import { configureStore } from '@reduxjs/toolkit';
import { bwellApi } from '../services/bwellApi';
import authReducer from '../features/auth/authSlice';
import checkoutReducer from '../features/checkout/checkoutSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    checkout: checkoutReducer,
    [bwellApi.reducerPath]: bwellApi.reducer,
  },
  middleware: (getDefault) => getDefault().concat(bwellApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
