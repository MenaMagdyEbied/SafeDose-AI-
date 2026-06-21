import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import {
  provideRouter,
  withHashLocation,
  withInMemoryScrolling,
  withViewTransitions,
} from '@angular/router';

import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import {
  Activity,
  Heart,
  LUCIDE_ICONS,
  LucideIconProvider,
  Pill,
  Shield,
  Sparkles,
  TriangleAlert,
} from 'lucide-angular';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth-interceptor';
import { loaderInterceptor } from './core/interceptors/loader-interceptor';
import { responseInterceptor } from './core/interceptors/response-interceptor';

const icons = { Shield, Pill, Sparkles, Activity, TriangleAlert, Heart };
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(
      routes,
      withInMemoryScrolling({ scrollPositionRestoration: 'top' }),
      withViewTransitions(),
      withHashLocation(),
    ),
    provideHttpClient(
      withFetch(),
      withInterceptors([loaderInterceptor, authInterceptor, responseInterceptor]),
    ),
    {
      provide: LUCIDE_ICONS,
      multi: true,
      useValue: new LucideIconProvider(icons),
    },
  ],
};
