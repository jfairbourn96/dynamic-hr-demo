import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import { employeesApi } from '../api/employees';
import { DynamicForm } from '../components/DynamicForm';
import type { UpdateEmployeeRequest } from '../types/records';

const CORE_FIELDS: { name: keyof Pick<UpdateEmployeeRequest, 'firstName' | 'lastName' | 'email' | 'hireDate' | 'department'>; label: string; type: string; required: boolean }[] = [
  { name: 'firstName', label: 'First Name', type: 'text', required: true },
  { name: 'lastName', label: 'Last Name', type: 'text', required: true },
  { name: 'email', label: 'Email', type: 'email', required: true },
  { name: 'hireDate', label: 'Hire Date', type: 'date', required: true },
  { name: 'department', label: 'Department', type: 'text', required: false },
];

const inputClass =
  'mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';

export function ViewEmployeePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [coreValues, setCoreValues] = useState<Record<string, string>>({});
  const [dynamicValues, setDynamicValues] = useState<Record<string, unknown>>({});

  const { data: employee, isLoading, isError } = useQuery({
    queryKey: ['employees', id],
    queryFn: () => employeesApi.getById(id!),
    enabled: !!id,
    refetchOnMount: 'always',
  });

  useEffect(() => {
    if (!employee) return;

    setCoreValues({
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email,
      hireDate: employee.hireDate,
      department: employee.department || '',
    });
    setDynamicValues(employee.fieldValues);
  }, [employee]);

  const updateMutation = useMutation({
    mutationFn: (request: UpdateEmployeeRequest) => employeesApi.update(id!, request),
    onSuccess: () => navigate('/employees/search'),
  });

  if (isLoading) return <p className="text-gray-500">Loading...</p>;

  if (isError || !employee) {
    return (
      <div>
        <p className="text-red-600">Failed to load employee record.</p>
        <button onClick={() => navigate(-1)} className="mt-2 text-sm text-blue-600 hover:underline">
          Back
        </button>
      </div>
    );
  }

  const employeeType = employee.employeeType;

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    updateMutation.mutate({
      firstName: coreValues.firstName || '',
      lastName: coreValues.lastName || '',
      email: coreValues.email || '',
      hireDate: coreValues.hireDate || '',
      department: coreValues.department || '',
      employeeTypeId: employee.employeeTypeId,
      fieldValues: dynamicValues,
    });
  };

  return (
    <div className="max-w-2xl">
      <div className="flex items-center gap-3 mb-6">
        <button onClick={() => navigate(-1)} className="text-sm text-blue-600 hover:underline">
          Back to results
        </button>
        {employeeType && (
          <span className="rounded-full bg-blue-100 px-3 py-0.5 text-xs font-medium text-blue-700">
            {employeeType.name}
          </span>
        )}
      </div>

      <h1 className="text-2xl font-bold text-gray-900 mb-6">Edit Employee</h1>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="rounded-lg border border-gray-200 p-4 space-y-4">
          <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide">
            Core Information
          </h2>
          {CORE_FIELDS.map((field) => (
            <div key={field.name}>
              <label className="block text-sm font-medium text-gray-700">
                {field.label}
                {field.required && <span className="ml-1 text-red-500">*</span>}
              </label>
              <input
                type={field.type}
                className={inputClass}
                value={coreValues[field.name] || ''}
                onChange={(event) =>
                  setCoreValues((previous) => ({ ...previous, [field.name]: event.target.value }))
                }
                required={field.required}
              />
            </div>
          ))}
        </div>

        {employeeType && employeeType.fields.length > 0 && (
          <div className="rounded-lg border border-gray-200 p-4">
            <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-4">
              {employeeType.name} Fields
            </h2>
            <DynamicForm
              fields={employeeType.fields}
              values={dynamicValues}
              onChange={(name, value) =>
                setDynamicValues((previous) => ({ ...previous, [name]: value }))
              }
            />
          </div>
        )}

        <div className="flex items-center gap-3">
          <button
            type="submit"
            disabled={updateMutation.isPending}
            className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>

        {updateMutation.isError && (
          <p className="text-sm text-red-600">{(updateMutation.error as Error).message}</p>
        )}
      </form>
    </div>
  );
}
