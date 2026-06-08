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
/*
 {
    path: 'splash',
    loadComponent: () => import('./views/splash/splash.component').then((m) => m.SplashComponent),
  },
  {
    path: 'auth',
    loadComponent: () => import('./views/auth/auth.component').then((m) => m.AuthComponent),
  },

  {
    path: 'interaction-checker',
    loadComponent: () =>
      import('./views/interaction-checker/interaction-checker.component').then(
        (m) => m.InteractionCheckerComponent,
      ),
    canActivate: [authGuard],
  },
  {
    path: 'interaction-results',
    loadComponent: () =>
      import('./views/interaction-results/interaction-results.component').then(
        (m) => m.InteractionResultsComponent,
      ),
    canActivate: [authGuard],
  },
  {
    path: 'digital-card',
    loadComponent: () =>
      import('./views/digital-card/digital-card.component').then((m) => m.DigitalCardComponent),
    canActivate: [authGuard],
  },

  {
    path: 'caregiver-review',
    loadComponent: () =>
      import('./views/caregiver-review/caregiver-review.component').then(
        (m) => m.CaregiverReviewComponent,
      ),
    canActivate: [authGuard],
  },
  {
    path: 'admin',
    loadComponent: () =>
      import('./views/admin-dashboard/admin-dashboard.component').then(
        (m) => m.AdminDashboardComponent,
      ),
    canActivate: [adminGuard],
  },

  {
    path: 'profile',
    loadComponent: () =>
      import('./views/profile/profile.component').then((m) => m.ProfileComponent),
    canActivate: [authGuard],
  },


*/
