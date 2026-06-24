import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { AdminManagement } from './admin-management';

describe('AdminManagement', () => {
  let service: AdminManagement;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(AdminManagement);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should list admins with pagination params', () => {
    service.getAdmins(2, 10).subscribe();

    const req = httpMock.expectOne(
      (request) =>
        request.method === 'GET' &&
        request.url.includes('/admin/admins') &&
        request.params.get('page') === '2' &&
        request.params.get('pageSize') === '10',
    );

    expect(req.request.method).toBe('GET');
    req.flush({ items: [] });
  });

  it('should create an admin with the documented payload', () => {
    const payload = { name: 'Ali', email: 'ali@example.com', password: '123456', role: 'admin' };

    service.createAdmin(payload).subscribe();

    const req = httpMock.expectOne(
      (request) => request.method === 'POST' && request.url.includes('/admin/admins'),
    );

    expect(req.request.body).toEqual(payload);
    req.flush({ id: '1' });
  });

  it('should update and delete admins and change status', () => {
    service
      .updateAdmin('1', {
        name: 'Ali',
        email: 'ali@example.com',
        role: 'admin',
        newPassword: 'new-pass',
      })
      .subscribe();
    service.deleteAdmin('2').subscribe();
    service.toggleAdminStatus('3', false).subscribe();

    const updateReq = httpMock.expectOne(
      (request) => request.method === 'PUT' && request.url.includes('/admin/admins/1'),
    );
    const deleteReq = httpMock.expectOne(
      (request) => request.method === 'DELETE' && request.url.includes('/admin/admins/2'),
    );
    const statusReq = httpMock.expectOne(
      (request) => request.method === 'PATCH' && request.url.includes('/admin/admins/3/status'),
    );

    expect(updateReq.request.body).toEqual({
      name: 'Ali',
      email: 'ali@example.com',
      role: 'admin',
      newPassword: 'new-pass',
    });
    expect(statusReq.request.body).toEqual({ isActive: false });

    updateReq.flush({ id: '1' });
    deleteReq.flush({});
    statusReq.flush({});
  });
});
