import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { limitGuard } from './limit-guard';

describe('limitGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => limitGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
