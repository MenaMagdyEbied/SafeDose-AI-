import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { authGuard } from './core/auth/guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: Home },
  {
    path: 'patient',
    loadComponent: () => import('./features/patient-home/patient-home').then((c) => c.PatientHome),
    canActivate: [authGuard],
  },
  {
    path: 'caregiver',
    loadComponent: () =>
      import('./features/caregiver-dashboard/caregiver-dashboard').then(
        (c) => c.CaregiverDashboard,
      ),
    canActivate: [authGuard],
  },
  {
    path: 'pricing',
    loadComponent: () => import('./features/pricing/pricing').then((c) => c.Pricing),
  },
  { path: '**', redirectTo: 'home' },
];
