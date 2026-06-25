import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Check, LucideAngularModule, Pencil, Plus, Shield, Trash2, User, X } from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';
import { Admin } from '../../../core/models/admin';
import { AdminManagement } from '../services/admin-management';

interface AdminFormState {
  name: string;
  email: string;
  role: Admin['role'];
  password: string;
  newPassword: string;
}

@Component({
  selector: 'app-admin-manager',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './admin-manager.html',
  styleUrl: './admin-manager.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminManager implements OnInit {
  private readonly adminService = inject(AdminManagement);
  private readonly authService = inject(Auth);
  private readonly cdr = inject(ChangeDetectorRef);

  plusIcon = Plus;
  editIcon = Pencil;
  trashIcon = Trash2;
  xIcon = X;
  checkIcon = Check;
  shieldIcon = Shield;
  userIcon = User;

  admins: Admin[] = [];
  isLoading = false;
  isSaving = false;
  isDeleting = false;
  isTogglingStatus = false;

  showForm = false;
  isEditing = false;
  showDeleteDialog = false;
  selectedAdmin: Admin | null = null;

  form: AdminFormState = {
    name: '',
    email: '',
    role: 'admin',
    password: '',
    newPassword: '',
  };

  roles = [
    { value: 'super-admin', label: 'سوبر أدمن' },
    { value: 'admin', label: 'أدمن' },
  ];

  ngOnInit() {
    this.loadAdmins();
  }

  get hasSuperAdmin() {
    return this.admins.some((admin) => admin.role === 'super-admin');
  }

  get availableRoles() {
    return this.hasSuperAdmin
      ? this.roles.filter((role) => role.value !== 'super-admin')
      : this.roles;
  }

  canEdit(admin: Admin) {
    return this.canManageAdmin(admin);
  }

  canDelete(admin: Admin) {
    return this.canManageAdmin(admin);
  }

  canToggleStatus(admin: Admin) {
    return this.canManageAdmin(admin);
  }

  loadAdmins() {
    this.isLoading = true;
    this.adminService.getAdmins().subscribe({
      next: (response) => {
        const rawAdmins = this.extractAdminsFromResponse(response);
        const mappedAdmins = rawAdmins.map((admin) => this.mapAdmin(admin));
        this.admins = this.sortAdmins(mappedAdmins);
        this.isLoading = false;
        this.cdr.markForCheck();
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.admins = [];
        this.cdr.markForCheck();
        this.cdr.detectChanges();
      },
    });
  }

  openAdd() {
    this.isEditing = false;
    this.selectedAdmin = null;
    this.form = { name: '', email: '', role: 'admin', password: '', newPassword: '' };
    this.showForm = true;
  }

  openEdit(admin: Admin) {
    if (!this.canManageAdmin(admin)) {
      return;
    }
    this.isEditing = true;
    this.selectedAdmin = admin;
    this.form = {
      name: admin.name,
      email: admin.email,
      role: admin.role,
      password: '',
      newPassword: '',
    };
    this.showForm = true;
  }

  save() {
    if (this.selectedAdmin && !this.canManageAdmin(this.selectedAdmin)) {
      this.showForm = false;
      this.selectedAdmin = null;
      this.isEditing = false;
      return;
    }

    if (!this.form.name || !this.form.email) {
      return;
    }

    if (!this.isEditing && !this.form.password) {
      return;
    }

    const roleToSend = this.isEditing
      ? this.normalizeRole(this.selectedAdmin?.role ?? this.form.role)
      : this.getRoleForNewAdmin();

    const payload: Record<string, unknown> = {
      name: this.form.name,
      email: this.form.email,
      role: roleToSend,
    };

    if (!this.isEditing) {
      payload['password'] = this.form.password;
    } else if (this.form.newPassword) {
      payload['newPassword'] = this.form.newPassword;
    }

    this.isSaving = true;

    const request =
      this.isEditing && this.selectedAdmin
        ? this.adminService.updateAdmin(this.selectedAdmin.id, payload)
        : this.adminService.createAdmin(payload);

    request.subscribe({
      next: () => {
        this.isSaving = false;
        this.showForm = false;
        this.selectedAdmin = null;
        this.isEditing = false;
        this.loadAdmins();
        this.cdr.markForCheck();
        this.cdr.detectChanges();
      },
      error: () => {
        this.isSaving = false;
        this.cdr.markForCheck();
        this.cdr.detectChanges();
      },
    });
  }

  confirmDelete(admin: Admin) {
    if (!this.canManageAdmin(admin)) {
      return;
    }
    this.selectedAdmin = admin;
    this.showDeleteDialog = true;
  }

  executeDelete() {
    if (!this.selectedAdmin) {
      return;
    }

    this.isDeleting = true;
    this.adminService.deleteAdmin(this.selectedAdmin.id).subscribe({
      next: () => {
        this.admins = this.admins.filter((admin) => admin.id !== this.selectedAdmin?.id);
        this.showDeleteDialog = false;
        this.selectedAdmin = null;
        this.isDeleting = false;
      },
      error: () => {
        this.isDeleting = false;
      },
    });
  }

  toggleStatus(admin: Admin) {
    if (!this.canManageAdmin(admin)) {
      return;
    }

    this.isTogglingStatus = true;
    this.adminService.toggleAdminStatus(admin.id, admin.status !== 'active').subscribe({
      next: () => {
        this.admins = this.admins.map((item) =>
          item.id === admin.id
            ? { ...item, status: admin.status === 'active' ? 'inactive' : 'active' }
            : item,
        );
        this.isTogglingStatus = false;
      },
      error: () => {
        this.isTogglingStatus = false;
      },
    });
  }

  getRoleLabel(role: string) {
    return this.roles.find((item) => item.value === role)?.label ?? role;
  }

  getStatusLabel(status: string) {
    return status === 'active' ? 'نشط' : 'غير نشط';
  }

  private canManageAdmin(admin: Admin): boolean {
    if (!this.authService.isSuperAdmin) {
      return true;
    }

    const currentUser = this.authService.user;
    const currentEmail = currentUser?.email?.trim().toLowerCase();
    const currentUserName = currentUser?.userName?.trim().toLowerCase();
    const currentName = currentUser?.name?.trim().toLowerCase();

    const targetEmail = admin.email?.trim().toLowerCase();
    const targetUserName = (admin as Admin & { userName?: string }).userName?.trim().toLowerCase();
    const targetName = admin.name?.trim().toLowerCase();

    const matchesIdentity =
      (currentEmail && targetEmail && currentEmail === targetEmail) ||
      (currentUserName && targetUserName && currentUserName === targetUserName) ||
      (currentName && targetName && currentName === targetName);

    return !matchesIdentity;
  }

  private getRoleForNewAdmin(): string {
    const hasSuperAdmin = this.admins.some((admin) => admin.role === 'super-admin');
    return hasSuperAdmin ? 'Admin' : 'SuperAdmin';
  }

  private sortAdmins(admins: Admin[]): Admin[] {
    return [...admins].sort((a, b) => {
      const aIsSuper = a.role === 'super-admin';
      const bIsSuper = b.role === 'super-admin';

      if (aIsSuper && !bIsSuper) return -1;
      if (!aIsSuper && bIsSuper) return 1;

      return (a.name || '').localeCompare(b.name || '', 'ar', { sensitivity: 'base' });
    });
  }

  private normalizeRole(role: string): string {
    const normalized = role?.trim().toLowerCase().replace(/\s+/g, '-');
    if (normalized.includes('super')) {
      return 'SuperAdmin';
    }
    if (normalized.includes('admin') || normalized.includes('administrator')) {
      return 'Admin';
    }
    return 'Admin';
  }

  private normalizeUiRole(role: string): Admin['role'] {
    const normalized = role?.trim().toLowerCase().replace(/\s+/g, '-');
    if (normalized.includes('super')) {
      return 'super-admin';
    }
    if (normalized.includes('moderator')) {
      return 'moderator';
    }
    if (normalized.includes('admin') || normalized.includes('administrator')) {
      return 'admin';
    }
    return 'admin';
  }

  private extractAdminsFromResponse(response: unknown): unknown[] {
    if (Array.isArray(response)) {
      return response;
    }

    const queue: unknown[] = [response];
    const visited = new Set<unknown>();

    while (queue.length) {
      const current = queue.shift();
      if (Array.isArray(current)) {
        return current;
      }

      if (current && typeof current === 'object' && !visited.has(current)) {
        visited.add(current);
        queue.push(...Object.values(current as Record<string, unknown>));
      }
    }

    return [];
  }

  private mapAdmin(admin: unknown): Admin {
    const source = admin as Record<string, unknown>;
    const role = String(source['role'] ?? source['roleName'] ?? source['userRole'] ?? 'admin');
    const status =
      source['isActive'] === false || source['status'] === 'inactive' ? 'inactive' : 'active';

    return {
      id: String(source['id'] ?? source['adminId'] ?? ''),
      name: String(source['name'] ?? source['fullName'] ?? source['userName'] ?? ''),
      email: String(source['email'] ?? source['emailAddress'] ?? ''),
      role: this.normalizeUiRole(role),
      status: status as Admin['status'],
      createdAt: String(source['createdAt'] ?? source['created_at'] ?? ''),
    };
  }
}
