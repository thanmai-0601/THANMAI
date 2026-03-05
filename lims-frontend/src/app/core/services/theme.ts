import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private isDarkSubject = new BehaviorSubject<boolean>(false);
  isDark$ = this.isDarkSubject.asObservable();

  constructor() {
    this.initTheme();
  }

  private initTheme(): void {
    const stored = localStorage.getItem('NexaLife_theme');
    if (stored === 'dark') {
      this.setDark(true);
    } else {
      this.setDark(false);
    }
  }

  toggleTheme(): void {
    this.setDark(!this.isDarkSubject.value);
  }

  private setDark(dark: boolean): void {
    this.isDarkSubject.next(dark);
    if (dark) {
      document.documentElement.classList.add('dark');
      localStorage.setItem('NexaLife_theme', 'dark');
    } else {
      document.documentElement.classList.remove('dark');
      localStorage.setItem('NexaLife_theme', 'light');
    }
  }

  isDark(): boolean {
    return this.isDarkSubject.value;
  }
}
