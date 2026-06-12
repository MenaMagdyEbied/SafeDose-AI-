import { Routes } from '@angular/router';
import { AdminDashboard } from './features/admin-dashboard/admin-dashboard';
import { CaregiverDashboard } from './features/caregiver-dashboard/caregiver-dashboard';
import { CaregiverReview } from './features/caregiver-review/caregiver-review';
import { DigitalCard } from './features/digital-card/digital-card';
import { Home } from './features/home/home';
import { InteractionChecker } from './features/interaction-checker/interaction-checker';
import { InteractionResults } from './features/interaction-results/interaction-results';
import { PatientHome } from './features/patient-home/patient-home';
import { Pricing } from './features/pricing/pricing';
import { Profile } from './features/profile/profile';
import { Splash } from './features/splash/splash';
import { NotFound } from './shared/components/not-found/not-found';
import { MainLayout } from './layouts/main-layout/main-layout';
import { Register } from './features/register/register';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  {
    path: '',
    component: MainLayout,
    children: [
      {
        path: 'home',
        component: Home,
      },
      {
        path: 'splash',
        component: Splash,
      },

      {
        path: 'auth',
        component: Register,
      },
      {
        path: 'patient',
        component: PatientHome,
      },
      {
        path: 'interaction-checker',
        component: InteractionChecker,
      },
      {
        path: 'interaction-results',
        component: InteractionResults,
      },
      {
        path: 'digital-card',
        component: DigitalCard,
      },
      {
        path: 'caregiver',
        component: CaregiverDashboard,
      },
      {
        path: 'caregiver-review',
        component: CaregiverReview,
      },
      {
        path: 'admin',
        component: AdminDashboard,
      },
      {
        path: 'pricing',
        component: Pricing,
      },
      {
        path: 'profile',
        component: Profile,
      },
    ],
  },
  { path: '**', component: NotFound },
];
