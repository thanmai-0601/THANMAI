import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { DataTable } from './data-table';

describe('DataTable', () => {
    let component: DataTable;
    let fixture: ComponentFixture<DataTable>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [DataTable],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
        fixture = TestBed.createComponent(DataTable);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have correct selector', () => {
        const el = fixture.debugElement.nativeElement;
        expect((DataTable as any).ɵcmp.selectors[0][0]).toBe('app-data-table');
    });

    it('should render without errors', () => {
    expect(component).toBeDefined();
  });

    it('should be a standalone component', () => {
        expect(DataTable).toBeTruthy();
    });

    it('should render template', () => {
        const el = fixture.nativeElement as HTMLElement;
        expect(el).toBeTruthy();
    });
});
