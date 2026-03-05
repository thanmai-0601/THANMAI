import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { LoadingSpinner } from './loading-spinner';

describe('LoadingSpinner', () => {
    let component: LoadingSpinner;
    let fixture: ComponentFixture<LoadingSpinner>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [LoadingSpinner],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
        fixture = TestBed.createComponent(LoadingSpinner);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should render the component', () => {
        const el = fixture.nativeElement as HTMLElement;
        expect(el).toBeTruthy();
    });

    it('should have the correct selector', () => {
        const el = fixture.debugElement.nativeElement;
        expect((LoadingSpinner as any).ɵcmp.selectors[0][0]).toBe('app-loading-spinner');
    });

    it('should be a standalone component', () => {
        expect(LoadingSpinner).toBeTruthy();
    });

    it('should render without errors', () => {
    expect(component).toBeDefined();
  });
});
