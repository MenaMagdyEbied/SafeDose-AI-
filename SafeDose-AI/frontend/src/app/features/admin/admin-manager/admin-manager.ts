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

  // UI role values يتطابقوا مع normalizeUiRole output
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
    if (!this.canManageAdmin(admin)) return;
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

    if (!this.form.name || !this.form.email) return;
    if (!this.isEditing && !this.form.password) return;

    // لو editing → نحتفظ بالـ role الأصلي، لو adding → نحدد الـ role المناسب
    const roleToSend = this.isEditing
      ? this.toApiRole(this.selectedAdmin?.role ?? this.form.role)
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
    if (!this.canManageAdmin(admin)) return;
    this.selectedAdmin = admin;
    this.showDeleteDialog = true;
  }

  executeDelete() {
    if (!this.selectedAdmin) return;

    this.isDeleting = true;
    this.adminService.deleteAdmin(this.selectedAdmin.id).subscribe({
      next: () => {
        this.admins = this.admins.filter((a) => a.id !== this.selectedAdmin?.id);
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
    if (!this.canManageAdmin(admin)) return;

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

  getRoleLabel(role: string): string {
    // role هنا بييجي من normalizeUiRole → 'super-admin' أو 'admin'
    return this.roles.find((r) => r.value === role)?.label ?? role;
  }

  getStatusLabel(status: string): string {
    return status === 'active' ? 'نشط' : 'غير نشط';
  }

  // ─── Private ───────────────────────────────────────────────

  private canManageAdmin(admin: Admin): boolean {
    // لازم يكون SuperAdmin عشان يقدر يدير
    if (!this.authService.isSuperAdmin) return false;

    // SuperAdmin مش يقدر يعدل/يحذف نفسه
    const currentEmail = this.authService.user?.email?.trim().toLowerCase();
    const targetEmail = admin.email?.trim().toLowerCase();
    return !(currentEmail && targetEmail && currentEmail === targetEmail);
  }

  private getRoleForNewAdmin(): string {
    return 'Admin'; // ✅ دايماً Admin، السوبر أدمن بيتعمل manually من الباك إند
  }
  // UI ('super-admin' | 'admin') → API ('SuperAdmin' | 'Admin')
  private toApiRole(uiRole: string): string {
    const n = uiRole.trim().toLowerCase().replace(/\s+/g, '-');
    if (n.includes('super')) return 'SuperAdmin';
    return 'Admin';
  }

  // API role string → UI role ('super-admin' | 'admin' | 'moderator')
  private normalizeUiRole(role: string): Admin['role'] {
    const n = role.trim().toLowerCase().replace(/\s+/g, '-');
    if (n.includes('super')) return 'super-admin';
    return 'admin';
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

  private extractAdminsFromResponse(response: unknown): unknown[] {
    if (Array.isArray(response)) return response;

    const queue: unknown[] = [response];
    const visited = new Set<unknown>();

    while (queue.length) {
      const current = queue.shift();
      if (Array.isArray(current)) return current;
      if (current && typeof current === 'object' && !visited.has(current)) {
        visited.add(current);
        queue.push(...Object.values(current as Record<string, unknown>));
      }
    }

    return [];
  }

  private mapAdmin(admin: unknown): Admin {
    const s = admin as Record<string, unknown>;
    const role = String(s['role'] ?? s['roleName'] ?? s['userRole'] ?? 'admin');
    const status = s['isActive'] === false || s['status'] === 'inactive' ? 'inactive' : 'active';

    return {
      id: String(s['id'] ?? s['adminId'] ?? ''),
      name: String(s['name'] ?? s['fullName'] ?? s['userName'] ?? ''),
      email: String(s['email'] ?? s['emailAddress'] ?? ''),
      role: this.normalizeUiRole(role),
      status: status as Admin['status'],
      createdAt: String(s['createdAt'] ?? s['created_at'] ?? ''),
    };
  }
}
