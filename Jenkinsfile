pipeline {
    agent any
    
    tools {
        dotnetsdk 'dotnet-9.0'
    }
    
    environment {
        SONAR_HOST_URL = 'http://localhost:9000'
        DOCKER_REGISTRY = 'localhost:5001'
        IMAGE_NAME = 'voting-app'
        IMAGE_TAG = "${env.BUILD_ID}"
        DOTNET_CLI_HOME = "/tmp"
        APP_PORT = "8086"
    }
    
    stages {
        stage('Checkout') {
            steps {
                git branch: 'main',
                    url: 'https://github.com/atechsphere/sample-voting-dotnet-app-v1.0.git'
            }
        }
        
        stage('Install Tools') {
            steps {
                script {
                    sh '''
                        echo "=== Checking and Installing Tools ==="
                        
                        echo "1. Checking SonarScanner..."
                        dotnet tool list --global | grep sonarscanner || \
                        dotnet tool install --global dotnet-sonarscanner
                        
                        echo "2. Checking ReportGenerator..."
                        dotnet tool list --global | grep reportgenerator || \
                        dotnet tool install --global dotnet-reportgenerator-globaltool
                        
                        echo "3. Docker Compose:"
                        docker compose version
                    '''
                }
            }
        }
        
        stage('Restore & Build') {
            steps {
                dir('src') {
                    sh '''
                        echo "=== Restoring and Building ==="
                        dotnet restore VotingAppSolution.sln
                        dotnet build VotingAppSolution.sln --configuration Release --no-restore
                    '''
                }
            }
        }
        
        stage('Run Tests with Coverage') {
            steps {
                dir('src') {
                    script {
                        sh '''
                            echo "=== Running Tests ==="
                            dotnet test VotingApp.Tests/VotingApp.Tests.csproj \
                              --configuration Release \
                              --logger "trx;LogFileName=test-results.trx" \
                              --collect:"XPlat Code Coverage" \
                              --results-directory ./TestResults \
                              --settings coverlet.runsettings \
                              --verbosity normal
                        '''
                        
                        sh '''
                            echo "=== Processing Coverage ==="
                            mkdir -p TestResults/CoverageReport
                            
                            if find . -name "coverage.cobertura.xml" -type f | grep -q .; then
                                echo "Generating coverage reports"
                                reportgenerator \
                                  -reports:./**/coverage.cobertura.xml \
                                  -targetdir:./TestResults/CoverageReport \
                                  -reporttypes:HtmlInline
                                
                                reportgenerator \
                                  -reports:./**/coverage.cobertura.xml \
                                  -targetdir:./TestResults \
                                  -reporttypes:OpenCover
                                
                                echo "Coverage files generated successfully"
                            else
                                echo "WARNING: No coverage files found"
                                echo "<?xml version='1.0'?><CoverageSession/>" > TestResults/coverage.opencover.xml
                            fi
                        '''
                    }
                }
            }
            
            post {
                always {
                    junit 'src/**/TestResults/*.trx'
                    
                    script {
                        if (fileExists('src/TestResults/CoverageReport/index.html')) {
                            publishHTML(target: [
                                reportDir: 'src/TestResults/CoverageReport',
                                reportFiles: 'index.html',
                                reportName: 'Code Coverage Report',
                                keepAll: true
                            ])
                        }
                    }
                }
            }
        }
        
        stage('SonarQube Analysis') {
            when {
                expression { 
                    fileExists('src/TestResults/coverage.opencover.xml') && 
                    env.SONAR_HOST_URL != null 
                }
            }
            
            steps {
                script {
                    echo "=== Checking SonarQube Connection ==="
                    
                    // Test SonarQube connectivity
                    sh '''
                        echo "Testing connection to ${SONAR_HOST_URL}"
                        curl -f ${SONAR_HOST_URL}/api/system/status || echo "SonarQube not reachable"
                    '''
                }
                
                dir('src') {
                    withSonarQubeEnv('SonarQube-Local') {
                        script {
                            echo "=== Starting SonarQube Analysis ==="
                            
                            sh '''
                                # Start analysis
                                dotnet sonarscanner begin \
                                  /k:"sample-voting-dotnet-app-v1.0" \
                                  /n:"Voting Application" \
                                  /v:"${BUILD_ID}" \
                                  /d:sonar.host.url="${SONAR_HOST_URL}" \
                                  /d:sonar.cs.vstest.reportsPaths="TestResults/*.trx" \
                                  /d:sonar.cs.opencover.reportsPaths="TestResults/coverage.opencover.xml" \
                                  /d:sonar.coverage.exclusions="**Tests*.cs,**/Migrations/**" \
                                  /d:sonar.exclusions="**/wwwroot/lib/**,**/node_modules/**" \
                                  /d:sonar.verbose=true
                                
                                # Build with analysis
                                dotnet build VotingAppSolution.sln --configuration Release
                                
                                # End analysis
                                dotnet sonarscanner end
                            '''
                        }
                    }
                }
            }
            
            post {
                success {
                    echo "SonarQube analysis completed successfully"
                    echo "Dashboard: ${SONAR_HOST_URL}/dashboard?id=sample-voting-dotnet-app-v1.0"
                }
                failure {
                    echo "SonarQube analysis failed - continuing pipeline"
                    // Don't fail the entire pipeline for SonarQube issues
                }
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    echo "=== Building Docker Image ==="
                    docker.build("${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}", "--file docker/Dockerfile .")
                }
            }
        }
        
        stage('Push to Registry') {
            steps {
                script {
                    echo "=== Pushing to Registry ==="
                    docker.withRegistry("http://${DOCKER_REGISTRY}") {
                        docker.image("${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}").push()
                        docker.image("${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}").push('latest')
                    }
                }
            }
        }
        
        stage('Deploy') {
            steps {
                script {
                    sh '''
                        echo "=== Deploying Application ==="
                        
                        docker compose down || true
                        
                        docker compose up -d --build voting-app mysql-db
                        
                        sleep 10
                        docker compose ps
                    '''
                }
            }
        }
        
        stage('Health Check') {
            steps {
                script {
                    sh """
                        echo "=== Health Check ==="
                        
                        for i in {1..10}; do
                            if curl -f http://localhost:${APP_PORT}/health 2>/dev/null; then
                                echo "Application healthy on port ${APP_PORT}"
                                exit 0
                            fi
                            echo "Attempt \$i/10: Waiting..."
                            sleep 5
                        done
                        
                        echo "Health check timeout"
                        docker compose logs voting-app --tail=10
                        exit 0
                    """
                }
            }
        }
    }
    
    post {
        always {
            script {
                // Archive test results
                archiveArtifacts artifacts: 'src/**/TestResults/*.trx,src/**/TestResults/*.xml', fingerprint: true
                
                // Clean workspace
                cleanWs()
            }
        }
        
        success {
            echo '=== Pipeline Successful ==='
            
            script {
                echo "🌐 Application URL: http://localhost:${APP_PORT}"
                echo "🐳 Registry: http://${DOCKER_REGISTRY}"
                
                if (env.SONAR_HOST_URL) {
                    echo "📊 SonarQube: ${SONAR_HOST_URL}/dashboard?id=sample-voting-dotnet-app-v1.0"
                }
                
                sh '''
                    echo ""
                    echo "=== Service Status ==="
                    docker compose ps
                    echo ""
                    echo "=== Useful Commands ==="
                    echo "View logs: docker compose logs -f voting-app"
                    echo "Stop: docker compose down"
                    echo "Restart: docker compose restart voting-app"
                '''
            }
        }
        
        failure {
            echo '=== Pipeline Failed ==='
            
            script {
                sh '''
                    echo "=== Debug Information ==="
                    echo "Docker Compose:"
                    docker compose version
                    echo ""
                    echo "Docker Images:"
                    docker images | grep voting-app || echo "No voting-app images"
                    echo ""
                    echo "Running Containers:"
                    docker ps
                    echo ""
                    echo "Registry Status:"
                    curl -s http://localhost:5001/v2/_catalog || echo "Registry not accessible"
                    echo ""
                    echo "SonarQube Status:"
                    curl -s http://localhost:9000/api/system/status || echo "SonarQube not accessible"
                '''
            }
        }
    }
}
