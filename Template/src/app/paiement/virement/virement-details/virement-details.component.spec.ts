import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VirementDetailsComponent } from './virement-details.component';

describe('VirementDetailsComponent', () => {
  let component: VirementDetailsComponent;
  let fixture: ComponentFixture<VirementDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ VirementDetailsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(VirementDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
