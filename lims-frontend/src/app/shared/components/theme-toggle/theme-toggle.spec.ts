import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ThemeToggle } from './theme-toggle';
import { ThemeService } from '../../../core/services/theme';

describe('ThemeToggle', () => {
    let component: ThemeToggle;
    let fixture: ComponentFixture<ThemeToggle>;

    beforeEach(async () => {
        localStorage.clear();
        await TestBed.configureTestingModule({
            imports: [ThemeToggle],
            providers: [ThemeService],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
        fixture = TestBed.createComponent(ThemeToggle);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have themeService injected', () => {
        expect(component.themeService).toBeTruthy();
    });

    it('should toggle theme via service', () => {
        const initial = component.themeService.isDark();
        component.themeService.toggleTheme();
        expect(component.themeService.isDark()).not.toBe(initial);
    });

    it('should have correct selector', () => {
        const el = fixture.debugElement.nativeElement;
        expect((ThemeToggle as any).ɵcmp.selectors[0][0]).toBe('app-theme-toggle');
    });

    it('should render without errors', () => {
    expect(component).toBeDefined();
  });
});
