import ast
import os
import sys
from typing import List, Dict, Any
from dataclasses import dataclass
from enum import Enum
import radon.complexity as radon
import bandit
from pylint import epylint as lint

class AnalysisCategory(Enum):
    QUALITY = "Quality"
    PERFORMANCE = "Performance"
    SECURITY = "Security"
    BEST_PRACTICES = "Best Practices"
    ARCHITECTURE = "Architecture"

@dataclass
class CodeIssue:
    category: AnalysisCategory
    severity: str
    message: str
    line_number: int
    file_path: str
    recommendation: str

class CodeReviewAgent:
    def __init__(self):
        self.issues: List[CodeIssue] = []

    def analyze_code_quality(self, file_path: str) -> None:
        """Analyze code quality using various metrics."""
        with open(file_path, 'r', encoding='utf-8') as file:
            code = file.read()
            
        # Parse the AST
        tree = ast.parse(code)
        
        # Analyze function complexity
        for node in ast.walk(tree):
            if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)):
                self._check_function_complexity(node, file_path)
                self._check_function_length(node, file_path)
                self._check_naming_conventions(node, file_path)

        # Run pylint for additional quality checks
        (pylint_stdout, _) = lint.py_run(file_path, return_std=True)
        self._process_pylint_output(pylint_stdout.getvalue(), file_path)

    def analyze_performance(self, file_path: str) -> None:
        """Analyze code for performance issues."""
        with open(file_path, 'r', encoding='utf-8') as file:
            code = file.read()
            
        tree = ast.parse(code)
        
        # Check for common performance anti-patterns
        for node in ast.walk(tree):
            # Check for inefficient list operations
            if isinstance(node, ast.ListComp):
                self._check_list_comprehension_efficiency(node, file_path)
            
            # Check for unnecessary memory usage
            if isinstance(node, ast.For):
                self._check_loop_efficiency(node, file_path)

    def analyze_security(self, file_path: str) -> None:
        """Analyze code for security vulnerabilities."""
        # Use bandit for security analysis
        bandit_results = bandit.run(file_path)
        
        for issue in bandit_results.get_issues():
            self.issues.append(CodeIssue(
                category=AnalysisCategory.SECURITY,
                severity=issue.severity,
                message=issue.text,
                line_number=issue.line_number,
                file_path=file_path,
                recommendation=self._get_security_recommendation(issue)
            ))

    def analyze_best_practices(self, file_path: str) -> None:
        """Analyze code for adherence to best practices."""
        with open(file_path, 'r', encoding='utf-8') as file:
            code = file.read()
            
        tree = ast.parse(code)
        
        # Check for documentation
        for node in ast.walk(tree):
            if isinstance(node, (ast.FunctionDef, ast.ClassDef)):
                self._check_documentation(node, file_path)
        
        # Check import organization
        self._check_import_organization(tree, file_path)

    def analyze_architecture(self, file_path: str) -> None:
        """Analyze code architecture and design patterns."""
        with open(file_path, 'r', encoding='utf-8') as file:
            code = file.read()
            
        tree = ast.parse(code)
        
        # Check for dependency injection patterns
        self._check_dependency_injection(tree, file_path)
        
        # Check for proper layer separation
        self._check_layer_separation(file_path)

    def _check_function_complexity(self, node: ast.FunctionDef, file_path: str) -> None:
        """Check if function is too complex using cyclomatic complexity."""
        complexity = radon.cc_visit(node)
        if complexity > 10:
            self.issues.append(CodeIssue(
                category=AnalysisCategory.QUALITY,
                severity="high",
                message=f"Function '{node.name}' is too complex (complexity: {complexity})",
                line_number=node.lineno,
                file_path=file_path,
                recommendation="Consider breaking down the function into smaller, more focused functions"
            ))

    def _check_function_length(self, node: ast.FunctionDef, file_path: str) -> None:
        """Check if function is too long."""
        length = len(node.body)
        if length > 20:
            self.issues.append(CodeIssue(
                category=AnalysisCategory.QUALITY,
                severity="medium",
                message=f"Function '{node.name}' is too long ({length} lines)",
                line_number=node.lineno,
                file_path=file_path,
                recommendation="Split the function into smaller, more focused functions"
            ))

    def _check_naming_conventions(self, node: Any, file_path: str) -> None:
        """Check if names follow PEP 8 conventions."""
        name = getattr(node, 'name', '')
        if isinstance(node, ast.FunctionDef) and not name.islower():
            self.issues.append(CodeIssue(
                category=AnalysisCategory.QUALITY,
                severity="low",
                message=f"Function name '{name}' does not follow PEP 8 naming conventions",
                line_number=node.lineno,
                file_path=file_path,
                recommendation="Use lowercase with words separated by underscores"
            ))

    def _check_list_comprehension_efficiency(self, node: ast.ListComp, file_path: str) -> None:
        """Check if list comprehension is used efficiently."""
        if isinstance(node.generators[0].iter, ast.Call):
            self.issues.append(CodeIssue(
                category=AnalysisCategory.PERFORMANCE,
                severity="medium",
                message="Potentially inefficient list comprehension",
                line_number=node.lineno,
                file_path=file_path,
                recommendation="Consider using generator expression for large sequences"
            ))

    def _check_loop_efficiency(self, node: ast.For, file_path: str) -> None:
        """Check for inefficient loop patterns."""
        if isinstance(node.iter, ast.Call):
            self.issues.append(CodeIssue(
                category=AnalysisCategory.PERFORMANCE,
                severity="low",
                message="Potential inefficient loop operation",
                line_number=node.lineno,
                file_path=file_path,
                recommendation="Consider using itertools or generator expressions"
            ))

    def _get_security_recommendation(self, issue: Any) -> str:
        """Generate security recommendations based on the issue."""
        recommendations = {
            "sql_injection": "Use parameterized queries or an ORM",
            "hardcoded_password": "Use environment variables or a secure secret manager",
            "eval_used": "Avoid using eval() - use safer alternatives"
        }
        return recommendations.get(issue.test_id, "Review OWASP guidelines for secure coding")

    def _check_documentation(self, node: ast.AST, file_path: str) -> None:
        """Check for proper documentation."""
        if not ast.get_docstring(node):
            self.issues.append(CodeIssue(
                category=AnalysisCategory.BEST_PRACTICES,
                severity="low",
                message=f"Missing docstring for {node.__class__.__name__.lower()} '{getattr(node, 'name', '')}'",
                line_number=node.lineno,
                file_path=file_path,
                recommendation="Add a descriptive docstring following Google style guide"
            ))

    def _check_import_organization(self, tree: ast.AST, file_path: str) -> None:
        """Check if imports are properly organized."""
        imports = [node for node in ast.walk(tree) if isinstance(node, (ast.Import, ast.ImportFrom))]
        
        # Check for proper import grouping
        if not self._are_imports_grouped(imports):
            self.issues.append(CodeIssue(
                category=AnalysisCategory.BEST_PRACTICES,
                severity="low",
                message="Imports are not properly grouped",
                line_number=imports[0].lineno if imports else 1,
                file_path=file_path,
                recommendation="Group imports in order: standard library, third-party, local"
            ))

    def _are_imports_grouped(self, imports: List[ast.AST]) -> bool:
        """Check if imports are properly grouped."""
        # Simple check - just verify they're all together
        if not imports:
            return True
            
        lines = [imp.lineno for imp in imports]
        return max(lines) - min(lines) == len(imports) - 1

    def _check_dependency_injection(self, tree: ast.AST, file_path: str) -> None:
        """Check for proper dependency injection patterns."""
        for node in ast.walk(tree):
            if isinstance(node, ast.ClassDef):
                self._check_class_dependencies(node, file_path)

    def _check_class_dependencies(self, node: ast.ClassDef, file_path: str) -> None:
        """Check class dependencies for proper injection."""
        # Look for direct instantiation in methods
        for child in ast.walk(node):
            if isinstance(child, ast.Call) and isinstance(child.func, ast.Name):
                self.issues.append(CodeIssue(
                    category=AnalysisCategory.ARCHITECTURE,
                    severity="medium",
                    message="Direct instantiation found - consider dependency injection",
                    line_number=child.lineno,
                    file_path=file_path,
                    recommendation="Use dependency injection instead of direct instantiation"
                ))

    def _check_layer_separation(self, file_path: str) -> None:
        """Check for proper separation of concerns."""
        # Simple check based on file location
        layers = ['controllers', 'services', 'models', 'repositories']
        current_layer = None
        
        for layer in layers:
            if layer in file_path:
                current_layer = layer
                break
                
        if current_layer:
            with open(file_path, 'r', encoding='utf-8') as file:
                content = file.read()
                for other_layer in layers:
                    if other_layer != current_layer and other_layer in content:
                        self.issues.append(CodeIssue(
                            category=AnalysisCategory.ARCHITECTURE,
                            severity="high",
                            message=f"Layer violation: {current_layer} accessing {other_layer}",
                            line_number=1,
                            file_path=file_path,
                            recommendation=f"Maintain proper layer separation - {current_layer} should not directly access {other_layer}"
                        ))

    def _process_pylint_output(self, output: str, file_path: str) -> None:
        """Process pylint output and convert to CodeIssues."""
        for line in output.split('\n'):
            if ':' in line:
                parts = line.split(':')
                if len(parts) >= 3:
                    try:
                        line_number = int(parts[1])
                        message = parts[2].strip()
                        self.issues.append(CodeIssue(
                            category=AnalysisCategory.QUALITY,
                            severity="medium",
                            message=message,
                            line_number=line_number,
                            file_path=file_path,
                            recommendation="Follow pylint recommendations for code quality"
                        ))
                    except ValueError:
                        continue

    def generate_report(self) -> Dict[str, Any]:
        """Generate a comprehensive report of all issues."""
        report = {category.value: [] for category in AnalysisCategory}
        
        for issue in self.issues:
            report[issue.category.value].append({
                "severity": issue.severity,
                "message": issue.message,
                "line_number": issue.line_number,
                "file_path": issue.file_path,
                "recommendation": issue.recommendation
            })
            
        return report

def review_csharp_file(file_path):
    """Review a C# file."""
    with open(file_path, 'r', encoding='utf-8') as file:
        content = file.read()
        
    issues = []
    
    # Check file length
    lines = content.splitlines()
    if len(lines) > 300:
        issues.append(CodeIssue(
            category=AnalysisCategory.QUALITY,
            severity="medium",
            message=f"File is too long ({len(lines)} lines)",
            line_number=1,
            file_path=file_path,
            recommendation="Consider splitting into smaller files"
        ))
    
    # Check method length
    current_method_lines = 0
    for i, line in enumerate(lines, 1):
        if 'public' in line and ('async' in line or 'void' in line or 'Task' in line):
            current_method_lines = 0
        current_method_lines += 1
        if current_method_lines > 50:
            issues.append(CodeIssue(
                category=AnalysisCategory.QUALITY,
                severity="medium",
                message="Method is too long",
                line_number=i-50,
                file_path=file_path,
                recommendation="Split into smaller methods"
            ))
    
    # Check for test method attributes
    for i, line in enumerate(lines, 1):
        if 'public' in line and 'Test' in line:
            if '[TestMethod]' not in lines[i-2]:
                issues.append(CodeIssue(
                    category=AnalysisCategory.BEST_PRACTICES,
                    severity="high",
                    message="Test method missing [TestMethod] attribute",
                    line_number=i,
                    file_path=file_path,
                    recommendation="Add [TestMethod] attribute to test methods"
                ))
    
    # Check naming conventions
    for i, line in enumerate(lines, 1):
        if 'class' in line:
            class_name = line.split('class')[1].strip().split()[0]
            if not class_name[0].isupper():
                issues.append(CodeIssue(
                    category=AnalysisCategory.QUALITY,
                    severity="medium",
                    message=f"Class name '{class_name}' should start with uppercase",
                    line_number=i,
                    file_path=file_path,
                    recommendation="Use PascalCase for class names"
                ))
    
    return issues

def main():
    if len(sys.argv) < 2:
        print("Usage: python code_review_agent.py <path_to_code>")
        sys.exit(1)

    code_path = sys.argv[1]
    agent = CodeReviewAgent()

    if os.path.isfile(code_path):
        files = [code_path]
    else:
        files = [os.path.join(root, file)
                for root, _, files in os.walk(code_path)
                for file in files if file.endswith(('.cs', '.py'))]

    for file_path in files:
        if file_path.endswith('.cs'):
            agent.issues.extend(review_csharp_file(file_path))
        else:
            agent.analyze_code_quality(file_path)
        agent.analyze_performance(file_path)
        agent.analyze_security(file_path)
        agent.analyze_best_practices(file_path)
        agent.analyze_architecture(file_path)

    report = agent.generate_report()
    
    # Print report in a structured format
    for category, issues in report.items():
        if issues:
            print(f"\n{category} Issues:")
            print("=" * (len(category) + 8))
            for issue in issues:
                print(f"\nSeverity: {issue['severity']}")
                print(f"File: {issue['file_path']}")
                print(f"Line: {issue['line_number']}")
                print(f"Issue: {issue['message']}")
                print(f"Recommendation: {issue['recommendation']}")

if __name__ == "__main__":
    main()