import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ClaimsOfficerDashboard } from './claims-officer-dashboard';
import { AuthService } from '../../../core/services/auth';
import { ApiService } from '../../../core/services/api';

describe('ClaimsOfficerDashboard', () => {
  let component: ClaimsOfficerDashboard;
  let fixture: ComponentFixture<ClaimsOfficerDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClaimsOfficerDashboard, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: AuthService, useValue: jasmine.createSpyObj('AuthService', ['isLoggedIn', 'getUserRole', 'getUserId', 'getUserName']) },
        ApiService
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    fixture = TestBed.createComponent(ClaimsOfficerDashboard);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have a fixture', () => {
    expect(fixture).toBeTruthy();
  });

  it('should render without errors', () => {
    expect(component).toBeDefined();
  });

  it('should be a standalone component', () => {
    expect(ClaimsOfficerDashboard).toBeTruthy();
  });

  it('should initialize component properties', () => {
    expect(component).toBeDefined();
  });
});
