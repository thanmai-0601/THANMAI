import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { NotificationService, NotificationDto } from '../../core/services/notification';
import { ThemeToggle } from '../../shared/components/theme-toggle/theme-toggle';

interface MenuItem { label: string; icon: string; link: string; }

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ThemeToggle],
  templateUrl: './layout.html',
  styleUrl: './layout.css'
})
export class Layout implements OnInit {
  isSidebarOpen = true;
  isMobile = false;

  userName = '';
  userEmail = '';
  userRole = '';
  menuItems: MenuItem[] = [];

  notifications: NotificationDto[] = [];
  unreadCount = 0;
  isNotificationMenuOpen = false;

  constructor(
    private auth: AuthService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.checkScreenSize();
    window.addEventListener('resize', () => this.checkScreenSize());
  }

  ngOnInit(): void {
    if (this.auth.isLoggedIn()) {
      this.userName = this.auth.getUserName() || '';
      this.userEmail = this.auth.getUserEmail() || '';
      this.userRole = this.auth.getUserRole() || '';
      this.setupMenu(this.userRole);
      this.loadNotifications();
    }
  }

  loadNotifications(): void {
    this.notificationService.getMyNotifications().subscribe({
      next: (data) => {
        this.notifications = data;
        this.unreadCount = this.notifications.filter(n => !n.isRead).length;
      },
      error: (err) => console.error('Failed to load notifications', err)
    });
  }

  toggleNotificationMenu(): void {
    this.isNotificationMenuOpen = !this.isNotificationMenuOpen;
    if (this.isNotificationMenuOpen) {
      this.isProfileMenuOpen = false;
    }
  }

  closeNotificationMenu(): void {
    this.isNotificationMenuOpen = false;
  }

  markAsRead(id: number, event: Event): void {
    event.stopPropagation();
    const notification = this.notifications.find(n => n.notificationId === id);
    if (notification && !notification.isRead) {
      this.notificationService.markAsRead(id).subscribe({
        next: () => {
          notification.isRead = true;
          this.unreadCount = Math.max(0, this.unreadCount - 1);
        }
      });
    }
  }

  checkScreenSize(): void {
    this.isMobile = window.innerWidth <= 768;
    if (this.isMobile) this.isSidebarOpen = false;
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSidebarMobile(): void {
    if (this.isMobile) this.isSidebarOpen = false;
  }

  isProfileMenuOpen = false;

  toggleProfileMenu(): void {
    this.isProfileMenuOpen = !this.isProfileMenuOpen;
    if (this.isProfileMenuOpen) {
      this.isNotificationMenuOpen = false;
    }
  }

  closeProfileMenu(): void {
    this.isProfileMenuOpen = false;
  }

  setupMenu(role: string): void {
    switch (role) {
      case 'Admin':
        this.menuItems = [
          { label: 'Dashboard', icon: '📊', link: '/app/dashboard/admin' },
          { label: 'All Policies', icon: '📑', link: '/app/all-policies' },
          { label: 'All Claims', icon: '🏥', link: '/app/all-claims' },
          { label: 'Endorsements', icon: '📝', link: '/app/admin/endorsements' },
          { label: 'User Management', icon: '👥', link: '/app/admin/users' },
          { label: 'Plan Management', icon: '⚙️', link: '/app/admin/plans' }
        ];
        break;
      case 'Agent':
        this.menuItems = [
          { label: 'Dashboard', icon: '📊', link: '/app/dashboard/agent' },
          { label: 'Assigned Policies', icon: '📑', link: '/app/agent-policies' },
          { label: 'Endorsements', icon: '📝', link: '/app/endorsement/pending' }
        ];
        break;
      case 'ClaimsOfficer':
        this.menuItems = [
          { label: 'Dashboard', icon: '📊', link: '/app/dashboard/claims-officer' },
          { label: 'Assigned Claims', icon: '🏥', link: '/app/officer-claims' }
        ];
        break;
      case 'Customer':
        this.menuItems = [
          { label: 'Dashboard', icon: '📊', link: '/app/dashboard/customer' },
          { label: 'Browse Plans', icon: '🔎', link: '/app/plans' },
          { label: 'My Policies', icon: '📑', link: '/app/my-policies' },
          { label: 'My Payments', icon: '💳', link: '/app/my-payments' },
          { label: 'My Claims', icon: '🏥', link: '/app/my-claims' },
          { label: 'My Endorsements', icon: '📝', link: '/app/endorsement/my-endorsements' }
        ];
        break;
    }
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
