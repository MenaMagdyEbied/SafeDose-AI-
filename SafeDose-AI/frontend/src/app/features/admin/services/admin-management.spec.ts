import { TestBed } from '@angular/core/testing';

import { AdminManagement } from './admin-management';

describe('AdminManagement', () => {
  let service: AdminManagement;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminManagement);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
